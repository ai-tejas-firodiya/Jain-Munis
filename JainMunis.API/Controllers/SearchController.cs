using Microsoft.AspNetCore.Mvc;
using JainMunis.API.Models.DTOs;
using JainMunis.API.Services;
using System.ComponentModel.DataAnnotations;

namespace JainMunis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISaintService _saintService;
    private readonly ILocationService _locationService;
    private readonly IScheduleService _scheduleService;

    public SearchController(
        ISaintService saintService,
        ILocationService locationService,
        IScheduleService scheduleService)
    {
        _saintService = saintService;
        _locationService = locationService;
        _scheduleService = scheduleService;
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> GetNearbySaints(
        [FromQuery, Required] decimal latitude,
        [FromQuery, Required] decimal longitude,
        [FromQuery] int radiusKm = 50,
        [FromQuery] bool currentOnly = true)
    {
        try
        {
            if (radiusKm < 1 || radiusKm > 500)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Radius must be between 1 and 500 kilometers"
                    }
                });
            }

            var nearbySaints = await _saintService.GetNearbySaintsAsync(latitude, longitude, radiusKm);

            if (currentOnly)
            {
                // Filter to only include saints currently staying in the area
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                nearbySaints = nearbySaints
                    .Where(s => s.CurrentSchedule != null)
                    .ToList();
            }

            return Ok(new ApiResponse<List<ScheduleDto>> { Data = new List<ScheduleDto>() });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while searching for nearby saints",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("cities")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetCities([FromQuery] string? query = null)
    {
        try
        {
            var cities = await _locationService.GetCitiesAsync(query);
            return Ok(new ApiResponse<List<string>> { Data = cities });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching cities",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<ApiResponse<object>>> GetSearchSuggestions([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Search query is required"
                    }
                });
            }

            var searchTerm = query.ToLower();
            var cities = await _locationService.GetCitiesAsync(searchTerm);
            
            var suggestions = new
            {
                saints = new List<object>(),
                locations = new List<object>(),
                cities = cities.Take(10).ToList()
            };

            // TODO: Add saint and location name suggestions
            // This would require implementing search methods in the services

            return Ok(new ApiResponse<object> { Data = suggestions });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching search suggestions",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("advanced")]
    public async Task<ActionResult<ApiResponse<object>>> AdvancedSearch(
        [FromQuery] string? query = null,
        [FromQuery] string? city = null,
        [FromQuery] string? state = null,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null,
        [FromQuery] bool currentOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 20;
            if (limit > 100) limit = 100;

            // Search saints
            var saintSearchParams = new SearchParams
            {
                Search = query,
                City = city,
                IsActive = true
            };

            var (saints, saintTotal) = await _saintService.GetSaintsAsync(page, limit, saintSearchParams);

            var schedules = new List<ScheduleDto>();
            var locations = new List<LocationDto>();

            // Search schedules if date range provided
            if (dateFrom.HasValue || dateTo.HasValue || !string.IsNullOrWhiteSpace(city))
            {
                var scheduleSearchParams = new SearchParams
                {
                    City = city,
                    DateFrom = dateFrom?.ToDateTime(TimeOnly.MinValue),
                    DateTo = dateTo?.ToDateTime(TimeOnly.MaxValue)
                };

                if (currentOnly)
                {
                    schedules = await _scheduleService.GetCurrentSchedulesAsync(scheduleSearchParams);
                }
                else
                {
                    var (schedulesData, scheduleTotal) = await _scheduleService.GetSchedulesAsync(page, limit, scheduleSearchParams);
                    schedules = schedulesData;
                }
            }

            // Search locations if city or state provided
            if (!string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(state))
            {
                var locationSearchParams = new SearchParams
                {
                    City = city,
                    State = state
                };

                var (locationsData, locationTotal) = await _locationService.GetLocationsAsync(page, limit, locationSearchParams);
                locations = locationsData;
            }

            // Calculate total
            var total = saintTotal + schedules.Count + locations.Count;
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var result = new
            {
                saints,
                schedules,
                locations,
                pagination = new
                {
                    page,
                    limit,
                    total,
                    totalPages
                }
            };

            return Ok(new ApiResponse<object> { Data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred during advanced search",
                    Details = ex.Message
                }
            });
        }
    }
}
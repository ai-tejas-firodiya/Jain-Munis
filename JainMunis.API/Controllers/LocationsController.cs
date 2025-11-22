using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JainMunis.API.Models.DTOs;
using JainMunis.API.Services;

namespace JainMunis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LocationDto>>>> GetLocations(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] string? state = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 20;
            if (limit > 100) limit = 100;

            var searchParams = new SearchParams
            {
                Search = search,
                City = city,
                State = state
            };

            var (locations, total) = await _locationService.GetLocationsAsync(page, limit, searchParams);

            var response = new ApiResponse<List<LocationDto>>
            {
                Data = locations,
                Pagination = new PaginationDto
                {
                    Page = page,
                    Limit = limit,
                    Total = total
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching locations",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<LocationDto>>> GetLocation(Guid id)
    {
        try
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "LOCATION_NOT_FOUND",
                        Message = "Location not found"
                    }
                });
            }

            return Ok(new ApiResponse<LocationDto> { Data = location });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching the location",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<LocationDto>>> CreateLocation([FromBody] CreateLocationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Location name is required"
                    }
                });
            }

            if (string.IsNullOrWhiteSpace(request.Address))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Address is required"
                    }
                });
            }

            if (string.IsNullOrWhiteSpace(request.City))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "City is required"
                    }
                });
            }

            var location = await _locationService.CreateLocationAsync(request);
            return Ok(new ApiResponse<LocationDto> { Data = location });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "CREATION_ERROR",
                    Message = "An error occurred while creating the location",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<LocationDto>>> UpdateLocation(Guid id, [FromBody] UpdateLocationRequest request)
    {
        try
        {
            var location = await _locationService.UpdateLocationAsync(id, request);
            if (location == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "LOCATION_NOT_FOUND",
                        Message = "Location not found"
                    }
                });
            }

            return Ok(new ApiResponse<LocationDto> { Data = location });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "UPDATE_ERROR",
                    Message = "An error occurred while updating the location",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> DeleteLocation(Guid id)
    {
        try
        {
            var result = await _locationService.DeleteLocationAsync(id);
            if (!result)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "LOCATION_HAS_SCHEDULES",
                        Message = "Cannot delete location with existing schedules"
                    }
                });
            }

            return Ok(new ApiResponse<object> { Data = new { message = "Location deleted successfully" } });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "DELETION_ERROR",
                    Message = "An error occurred while deleting the location",
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

    [HttpGet("city/{city}")]
    public async Task<ActionResult<ApiResponse<List<LocationDto>>>> GetLocationsByCity(string city)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "City parameter is required"
                    }
                });
            }

            var locations = await _locationService.GetLocationsByCityAsync(city);
            return Ok(new ApiResponse<List<LocationDto>> { Data = locations });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching locations by city",
                    Details = ex.Message
                }
            });
        }
    }
}
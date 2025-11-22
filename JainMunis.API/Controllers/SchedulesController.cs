using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JainMunis.API.Models.DTOs;
using JainMunis.API.Services;

namespace JainMunis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> GetSchedules(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] Guid? saintId = null,
        [FromQuery] string? city = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 20;
            if (limit > 100) limit = 100;

            var searchParams = new SearchParams
            {
                SaintId = saintId,
                City = city,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var (schedules, total) = await _scheduleService.GetSchedulesAsync(page, limit, searchParams);

            var response = new ApiResponse<List<ScheduleDto>>
            {
                Data = schedules,
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
                    Message = "An error occurred while fetching schedules",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> GetSchedule(Guid id)
    {
        try
        {
            var schedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "SCHEDULE_NOT_FOUND",
                        Message = "Schedule not found"
                    }
                });
            }

            return Ok(new ApiResponse<ScheduleDto> { Data = schedule });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching the schedule",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> CreateSchedule([FromBody] CreateScheduleRequest request)
    {
        try
        {
            if (request.StartDate > request.EndDate)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "End date must be after start date"
                    }
                });
            }

            // Get current user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim?.Value;

            var schedule = await _scheduleService.CreateScheduleAsync(request, createdBy);
            return Ok(new ApiResponse<ScheduleDto> { Data = schedule });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "SCHEDULE_CONFLICT",
                    Message = ex.Message
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "CREATION_ERROR",
                    Message = "An error occurred while creating the schedule",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> UpdateSchedule(Guid id, [FromBody] UpdateScheduleRequest request)
    {
        try
        {
            // Validate date range if both dates are provided
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate.Value > request.EndDate.Value)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "End date must be after start date"
                    }
                });
            }

            // Get current user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var updatedBy = userIdClaim?.Value;

            var schedule = await _scheduleService.UpdateScheduleAsync(id, request, updatedBy);
            if (schedule == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "SCHEDULE_NOT_FOUND",
                        Message = "Schedule not found"
                    }
                });
            }

            return Ok(new ApiResponse<ScheduleDto> { Data = schedule });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "SCHEDULE_CONFLICT",
                    Message = ex.Message
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "UPDATE_ERROR",
                    Message = "An error occurred while updating the schedule",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSchedule(Guid id)
    {
        try
        {
            var result = await _scheduleService.DeleteScheduleAsync(id);
            if (!result)
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "SCHEDULE_NOT_FOUND",
                        Message = "Schedule not found"
                    }
                });
            }

            return Ok(new ApiResponse<object> { Data = new { message = "Schedule deleted successfully" } });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "DELETION_ERROR",
                    Message = "An error occurred while deleting the schedule",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("current")]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> GetCurrentSchedules(
        [FromQuery] string? city = null,
        [FromQuery] Guid? saintId = null)
    {
        try
        {
            var searchParams = new SearchParams
            {
                City = city,
                SaintId = saintId
            };

            var schedules = await _scheduleService.GetCurrentSchedulesAsync(searchParams);
            return Ok(new ApiResponse<List<ScheduleDto>> { Data = schedules });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching current schedules",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> GetUpcomingSchedules(
        [FromQuery] string? city = null,
        [FromQuery] Guid? saintId = null,
        [FromQuery] int daysAhead = 30)
    {
        try
        {
            if (daysAhead < 1 || daysAhead > 365)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Days ahead must be between 1 and 365"
                    }
                });
            }

            var searchParams = new SearchParams
            {
                City = city,
                SaintId = saintId,
                DaysAhead = daysAhead
            };

            var schedules = await _scheduleService.GetUpcomingSchedulesAsync(searchParams);
            return Ok(new ApiResponse<List<ScheduleDto>> { Data = schedules });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching upcoming schedules",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("saint/{saintId}")]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> GetSchedulesBySaint(Guid saintId)
    {
        try
        {
            var schedules = await _scheduleService.GetSchedulesBySaintAsync(saintId);
            return Ok(new ApiResponse<List<ScheduleDto>> { Data = schedules });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching schedules for the saint",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("overlap")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> CheckScheduleConflicts(
        [FromQuery] Guid saintId,
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid? excludeScheduleId = null)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "End date must be after start date"
                    }
                });
            }

            var conflicts = await _scheduleService.CheckOverlapsAsync(saintId, startDate, endDate, excludeScheduleId);
            return Ok(new ApiResponse<List<ScheduleDto>> { Data = conflicts });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while checking for schedule conflicts",
                    Details = ex.Message
                }
            });
        }
    }
}
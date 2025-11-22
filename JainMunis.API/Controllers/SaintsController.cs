using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JainMunis.API.Models.DTOs;
using JainMunis.API.Services;
using System.ComponentModel.DataAnnotations;

namespace JainMunis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaintsController : ControllerBase
{
    private readonly ISaintService _saintService;

    public SaintsController(ISaintService saintService)
    {
        _saintService = saintService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SaintDto>>>> GetSaints(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] bool? isActive = true)
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
                IsActive = isActive
            };

            var (saints, total) = await _saintService.GetSaintsAsync(page, limit, searchParams);

            var response = new ApiResponse<List<SaintDto>>
            {
                Data = saints,
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
                    Message = "An error occurred while fetching saints",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SaintDto>>> GetSaint(Guid id)
    {
        try
        {
            var saint = await _saintService.GetSaintByIdAsync(id);
            if (saint == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "SAINT_NOT_FOUND",
                        Message = "Saint not found"
                    }
                });
            }

            return Ok(new ApiResponse<SaintDto> { Data = saint });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching the saint",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SaintDto>>> CreateSaint([FromBody] CreateSaintRequest request)
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
                        Message = "Saint name is required"
                    }
                });
            }

            var saint = await _saintService.CreateSaintAsync(request);
            return Ok(new ApiResponse<SaintDto> { Data = saint });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "CREATION_ERROR",
                    Message = "An error occurred while creating the saint",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SaintDto>>> UpdateSaint(Guid id, [FromBody] UpdateSaintRequest request)
    {
        try
        {
            var saint = await _saintService.UpdateSaintAsync(id, request);
            if (saint == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "SAINT_NOT_FOUND",
                        Message = "Saint not found"
                    }
                });
            }

            return Ok(new ApiResponse<SaintDto> { Data = saint });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "UPDATE_ERROR",
                    Message = "An error occurred while updating the saint",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSaint(Guid id)
    {
        try
        {
            var result = await _saintService.DeleteSaintAsync(id);
            if (!result)
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "SAINT_NOT_FOUND",
                        Message = "Saint not found"
                    }
                });
            }

            return Ok(new ApiResponse<object> { Data = new { message = "Saint deleted successfully" } });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "DELETION_ERROR",
                    Message = "An error occurred while deleting the saint",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("city/{city}")]
    public async Task<ActionResult<ApiResponse<List<SaintDto>>>> GetSaintsByCity(string city)
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

            var saints = await _saintService.GetSaintsByCityAsync(city);
            return Ok(new ApiResponse<List<SaintDto>> { Data = saints });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching saints by city",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<ApiResponse<List<SaintDto>>>> GetNearbySaints(
        [FromQuery, Required] decimal latitude,
        [FromQuery, Required] decimal longitude,
        [FromQuery] int radiusKm = 50)
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

            var saints = await _saintService.GetNearbySaintsAsync(latitude, longitude, radiusKm);
            return Ok(new ApiResponse<List<SaintDto>> { Data = saints });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while fetching nearby saints",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpPost("{id}/photo")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> UpdateSaintPhoto(Guid id, IFormFile photo)
    {
        try
        {
            if (photo == null || photo.Length == 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Photo file is required"
                    }
                });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Only JPG, JPEG, PNG, and WebP files are allowed"
                    }
                });
            }

            // Validate file size (5MB max)
            if (photo.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "File size must be less than 5MB"
                    }
                });
            }

            var photoUrl = await _saintService.UpdateSaintPhotoAsync(id, photo);
            return Ok(new ApiResponse<string> { Data = photoUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "UPLOAD_ERROR",
                    Message = "An error occurred while uploading the photo",
                    Details = ex.Message
                }
            });
        }
    }
}
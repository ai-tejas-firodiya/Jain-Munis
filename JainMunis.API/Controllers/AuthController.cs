using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JainMunis.API.Models.DTOs;
using JainMunis.API.Services;

namespace JainMunis.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<Models.Entities.AdminUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(IAuthService authService, UserManager<Models.Entities.AdminUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _authService = authService;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Username and password are required"
                    }
                });
            }

            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                return Unauthorized(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "INVALID_CREDENTIALS",
                        Message = "Invalid username or password"
                    }
                });
            }

            return Ok(new ApiResponse<LoginResponse> { Data = response });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "LOGIN_ERROR",
                    Message = "An error occurred during login",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult<ApiResponse<object>> Logout()
    {
        try
        {
            // In a JWT-based system, logout is typically handled on the client side
            // by removing the token. The server could implement a token blacklist
            // if needed, but for now we'll just return a success response.
            return Ok(new ApiResponse<object> { Data = new { message = "Logged out successfully" } });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "LOGOUT_ERROR",
                    Message = "An error occurred during logout",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "INVALID_TOKEN",
                        Message = "Invalid authentication token"
                    }
                });
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "USER_NOT_FOUND",
                        Message = "User not found"
                    }
                });
            }

            return Ok(new ApiResponse<UserDto> { Data = user });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "PROFILE_ERROR",
                    Message = "An error occurred while fetching profile",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpPost("create-user")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            // Check if current user has permission to create users
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole != "super_admin")
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Username, email, and password are required"
                    }
                });
            }

            var existingUser = await _userManager.FindByNameAsync(request.Username);
            if (existingUser != null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "USERNAME_EXISTS",
                        Message = "Username already exists"
                    }
                });
            }

            var existingEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "EMAIL_EXISTS",
                        Message = "Email already exists"
                    }
                });
            }

            var user = new Models.Entities.AdminUser
            {
                UserName = request.Username,
                Email = request.Email,
                Role = request.Role,
                Permissions = request.Permissions,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "USER_CREATION_FAILED",
                        Message = "Failed to create user: " + errors
                    }
                });
            }

            var userDto = new UserDto
            {
                Id = Guid.Parse(user.Id),
                Username = user.UserName!,
                Email = user.Email!,
                Role = user.Role ?? "admin",
                IsActive = user.IsActive
            };

            return Ok(new ApiResponse<UserDto> { Data = userDto });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "USER_CREATION_ERROR",
                    Message = "An error occurred while creating the user",
                    Details = ex.Message
                }
            });
        }
    }

    [HttpGet("users")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            // Check if current user has permission to view users
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole != "super_admin")
            {
                return Forbid();
            }

            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                users = users.Where(u => u.Role == role);
            }

            if (isActive.HasValue)
            {
                users = users.Where(u => u.IsActive == isActive.Value);
            }

            var total = await users.CountAsync();
            var userList = await users
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var userDtos = userList.Select(u => new UserDto
            {
                Id = Guid.Parse(u.Id),
                Username = u.UserName!,
                Email = u.Email!,
                Role = u.Role ?? "admin",
                LastLogin = u.LastLogin,
                IsActive = u.IsActive
            }).ToList();

            return Ok(new ApiResponse<List<UserDto>>
            {
                Data = userDtos,
                Pagination = new PaginationDto
                {
                    Page = page,
                    Limit = limit,
                    Total = total
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "USERS_FETCH_ERROR",
                    Message = "An error occurred while fetching users",
                    Details = ex.Message
                }
            });
        }
    }
}
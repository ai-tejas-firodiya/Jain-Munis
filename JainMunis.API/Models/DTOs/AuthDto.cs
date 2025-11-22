using System.ComponentModel.DataAnnotations;

namespace JainMunis.API.Models.DTOs;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "admin";
    public string? Permissions { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? Permissions { get; set; }
    public bool? IsActive { get; set; }
}
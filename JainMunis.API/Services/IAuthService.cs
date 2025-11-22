using JainMunis.API.Models.DTOs;

namespace JainMunis.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task LogActivityAsync(Guid? adminUserId, string action, string? entityType, Guid? entityId, string? oldValues, string? newValues, string? ipAddress, string? userAgent);
}
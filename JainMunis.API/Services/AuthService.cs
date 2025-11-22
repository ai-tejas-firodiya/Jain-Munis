using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JainMunis.API.Data;
using JainMunis.API.Models.DTOs;
using JainMunis.API.Models.Entities;
using System.Text.Json;

namespace JainMunis.API.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly UserManager<AdminUser> _userManager;

    public AuthService(ApplicationDbContext context, IJwtService jwtService, UserManager<AdminUser> userManager)
    {
        _context = context;
        _jwtService = jwtService;
        _userManager = userManager;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            return null;
        }

        // Update last login
        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate JWT token
        var token = _jwtService.GenerateToken(user.UserName!, user.Id, user.Email!, user.Role ?? "admin");

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Role = user.Role ?? "admin",
            LastLogin = user.LastLogin,
            IsActive = user.IsActive
        };

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = userDto
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Role = user.Role ?? "admin",
            LastLogin = user.LastLogin,
            IsActive = user.IsActive
        };
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Role = user.Role ?? "admin",
            LastLogin = user.LastLogin,
            IsActive = user.IsActive
        };
    }

    public async Task LogActivityAsync(Guid? adminUserId, string action, string? entityType, Guid? entityId, string? oldValues, string? newValues, string? ipAddress, string? userAgent)
    {
        var activityLog = new ActivityLog
        {
            AdminUserId = adminUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();
    }
}
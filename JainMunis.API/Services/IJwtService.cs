using System.Security.Claims;

namespace JainMunis.API.Services;

public interface IJwtService
{
    string GenerateToken(string username, string userId, string email, string role);
    string? ValidateToken(string token);
    ClaimsPrincipal? GetPrincipalFromToken(string token);
}
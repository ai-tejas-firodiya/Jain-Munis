namespace JainMunis.API.Services;

public interface IJwtService
{
    string GenerateToken(string username, Guid userId, string email, string role);
    string? ValidateToken(string token);
    ClaimsPrincipal? GetPrincipalFromToken(string token);
}
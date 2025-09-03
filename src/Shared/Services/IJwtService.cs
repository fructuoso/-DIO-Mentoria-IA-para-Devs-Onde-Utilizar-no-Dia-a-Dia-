using System.Security.Claims;

namespace Shared.Services;

public interface IJwtService
{
    string GenerateToken(string userId, string role, TimeSpan? expiration = null);
    ClaimsPrincipal? ValidateToken(string token);
    string? GetUserIdFromToken(string token);
    string? GetRoleFromToken(string token);
}

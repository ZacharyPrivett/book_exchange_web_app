using BookExchange.Api.Auth.Entities;
using System.Security.Claims;

namespace BookExchange.Api.Auth.Services;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<RefreshToken> CreateRefreshTokenAsync(string token);
    Task<RefreshToken?> GetValidRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
}

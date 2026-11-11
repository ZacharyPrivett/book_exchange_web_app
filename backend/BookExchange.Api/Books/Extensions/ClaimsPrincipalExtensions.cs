using System.Security.Claims;

namespace BookExchange.Api.Books.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("User is not authenticated.");

    // Safe for anonymous-accessible endpoints (e.g. GET /books/{id}) where there may be no
    // logged-in user at all — returns null instead of throwing.
    public static string? GetUserIdOrDefault(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}

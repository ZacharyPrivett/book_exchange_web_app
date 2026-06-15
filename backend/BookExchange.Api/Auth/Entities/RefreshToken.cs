using System;

namespace BookExchange.Api.Auth.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public required string Token { get; set; }
    public required string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
}

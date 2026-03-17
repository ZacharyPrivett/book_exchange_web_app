using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;

namespace BookExchange.Api.Auth.Entities;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public UserProfile? Profile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

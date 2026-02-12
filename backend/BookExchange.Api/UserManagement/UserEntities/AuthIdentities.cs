
namespace BookExchange.Api.UserManagement.UserEntities;

public class AuthIdentity
{
    public int AuthId { get; set; }
    public int UserId { get; set; }  
    public User User { get; set; } = null!;
    public int ProviderId { get; set; }  // "local", "google", "microsoft", etc.
    public Provider? Provider { get; set; }
    public required string Identifier { get; set; }  // username or external provider ID
    public string? PasswordHash { get; set; }  // Only for "local" provider, null for OAuth
    public DateTime? LastLoginAt { get; set; }
    public bool IsPrimary { get; set; } = false;    

}
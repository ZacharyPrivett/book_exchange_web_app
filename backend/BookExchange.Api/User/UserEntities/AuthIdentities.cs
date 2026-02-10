using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookExchange.Api.User.UserEntities;

public class AuthIdentities
{
    public int AuthId { get; set; }
    public int UserId { get; set; }  
    
    // Navigation property to User
    public User User { get; set; } = null!;
    
    public required string Provider { get; set; }  // "local", "google", "microsoft", etc.
    public required string Identifier { get; set; }  // username or external provider ID
    public string? PasswordHash { get; set; }  // Only for "local" provider, null for OAuth
    public DateTime? LastLoginAt { get; set; }
    public bool IsPrimary { get; set; } = false;    

}

using System;

namespace BookExchange.Api.Auth.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    // Personal information
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    
    // Public profile
    public required string DisplayName { get; set; }  // Unique, visible to others (username/handle)
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Age { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

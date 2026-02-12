using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookExchange.Api.UserManagement.UserEntities;

public class UserProfile
{
    [Key][ForeignKey(nameof(User))] public int UserId { get; set; }
    public User User { get; set; } = null!;
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string AvitarUrl { get; set; }
    public required string PhoneNumber { get; set; }
    public required DateOnly DateOfBirth { get; set; }
    
}

using System;

namespace BookExchange.Api.User.UserEntities;

public class User
{
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime DeletedAt { get; set; }
    public UserProfile? UserProfile { get; set; }
    public ICollection<AuthIdentities> AuthIdentities { get; set; } = new List<AuthIdentities>();
}

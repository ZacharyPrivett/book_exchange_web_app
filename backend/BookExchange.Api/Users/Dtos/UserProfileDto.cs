namespace BookExchange.Api.Users.Dtos;

public record UserProfileDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? AvatarUrl,
    string? Age,
    List<String> Roles
);
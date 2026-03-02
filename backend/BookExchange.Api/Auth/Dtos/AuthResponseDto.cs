namespace BookExchange.Api.Auth.Dtos;

public record class AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    string UserId,
    string Email,
    string DisplayName,
    List<string> Roles
);

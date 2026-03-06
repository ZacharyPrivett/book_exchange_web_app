using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record RefreshTokenDto(
   [Required] string AccessToken,
   [Required] string RefreshToken
);

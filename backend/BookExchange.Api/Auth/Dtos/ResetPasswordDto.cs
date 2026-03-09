using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record ResetPasswordDto(
    [Required][EmailAddress] string Email,
    [Required] string Token,
    [Required][MinLength(10)] string NewPassword
);

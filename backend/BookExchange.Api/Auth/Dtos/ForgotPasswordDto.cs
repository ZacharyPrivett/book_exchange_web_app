using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record ForgotPasswordDto(
    [Required][EmailAddress] string Email
);


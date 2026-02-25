using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record class RegisterDto(
    [Required][EmailAddress] string Email,
    [Required][MinLength(10)] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string DisplayName,
    string? PhoneNumber,
    DateOnly? DateOnly 
);
using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record LoginDto(
   [Required][EmailAddress] string Email,
   [Required] string Password,
   bool RememeberMe = false 
);


using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.UserManagement.UserDtos;

public record class CreateUserDto
(
    [Required] DateTime CreatedAt,
    [Required] bool IsActive,
    DateTime? DeletedAt
);

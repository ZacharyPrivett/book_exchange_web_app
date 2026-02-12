using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.UserManagement.UserDtos;

public record class UpdateUserDto
(
    [Required] DateTime CreatedAt,
    [Required] bool IsActive,
    DateTime? DeleteAt
);

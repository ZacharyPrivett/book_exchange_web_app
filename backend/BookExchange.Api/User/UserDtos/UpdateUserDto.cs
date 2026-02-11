using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.User.UserDtos;

public record class UpdateUserDto
(
    [Required] DateTime CreatedAt,
    [Required] bool IsActive,
    DateTime? DeleteAt
);

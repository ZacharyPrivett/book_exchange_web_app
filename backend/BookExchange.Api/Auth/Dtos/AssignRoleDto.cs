using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Auth.Dtos;

public record AssignRoleDto(
    [Required] string UserId,
    [Required] string RoleName
);

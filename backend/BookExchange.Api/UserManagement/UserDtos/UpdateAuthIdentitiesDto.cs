using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.UserManagement.UserDtos;

public record class UpdateAuthIdentitiesDto
(
    int AuthId,
    int UserId,
    int ProviderId,
    [Required][StringLength(50)] string Identifier,
    string? PasswordHash,
    DateTime? LastLoginAt,
    [Required] bool IsPrimary
);

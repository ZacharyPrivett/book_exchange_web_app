using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.User.UserDtos;

public record class CreateUserProfileDto
(
    int UserId,
    [Required][StringLength(50)] string FirstName,
    [Required][StringLength(50)] string LastName,
    [Required][StringLength(50)] string Email,
    [Required][StringLength(50)] string DisplayName,
    [Required][StringLength(100)] string AvitarUrl,
    [Required][Phone] string PhoneNumber,
    [Required] DateOnly DateOfBirth
);

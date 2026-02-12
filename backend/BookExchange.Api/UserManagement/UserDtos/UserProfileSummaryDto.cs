namespace BookExchange.Api.UserManagement.UserDtos;

public record class UserProfileSummaryDto
(
    int UserId,
    string FirstName,
    string LastName,
    string Email,
    string DisplayName,
    string AvitarUrl,
    string PhoneNumber,
    DateOnly DateOfBirth
);

using System;
using BookExchange.Api.UserManagement.UserDtos;
using BookExchange.Api.UserManagement.UserEntities;

namespace BookExchange.Api.UserManagement.UserMapping;

public static class UserProfileMapping
{
    public static UserProfile ToEntity(this CreateUserProfileDto userProfile, int userId)
    {
        return new UserProfile()
        {
            UserId = userId,
            FirstName = userProfile.FirstName,
            LastName = userProfile.LastName,
            Email = userProfile.Email,
            DisplayName = userProfile.DisplayName,
            AvitarUrl = userProfile.AvitarUrl,
            PhoneNumber = userProfile.PhoneNumber,
            DateOfBirth = userProfile.DateOfBirth
        };
    }
    public static UserProfile ToEntity(this UpdateUserProfileDto userProfile, int userId)
    {
        return new UserProfile()
        {
            UserId = userId,
            FirstName = userProfile.FirstName,
            LastName = userProfile.LastName,
            Email = userProfile.Email,
            DisplayName = userProfile.DisplayName,
            AvitarUrl = userProfile.AvitarUrl,
            PhoneNumber = userProfile.PhoneNumber,
            DateOfBirth = userProfile.DateOfBirth
        };
    }
}

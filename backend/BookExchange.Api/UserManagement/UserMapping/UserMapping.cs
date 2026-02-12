using System;
using BookExchange.Api.UserManagement.UserDtos;
using BookExchange.Api.UserManagement.UserEntities;

namespace BookExchange.Api.UserManagement.UserMapping;

public static class UserMapping
{
    public static User ToEntity(this CreateUserDto user)
    {
        return new User()
        {
            CreatedAt = DateTime.Now,
            IsActive = user.IsActive,
        };
    }

    public static User ToEntity(this UpdateUserDto user, int userId)
    {
        return new User()
        {
            UserId = userId,
            CreatedAt = user.CreatedAt,
            IsActive = false,
            DeletedAt = DateTime.Now         
        };
    }
}

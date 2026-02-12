using System;
using BookExchange.Api.UserManagement.UserDtos;
using BookExchange.Api.UserManagement.UserEntities;

namespace BookExchange.Api.UserManagement.UserMapping;

public static class AuthIdentitiesMapping
{
    public static AuthIdentity ToEntity(this CreateAuthIdentitiesDto authIdentity)
    {
        return new AuthIdentity()
        {
            UserId = authIdentity.UserId,
            ProviderId = authIdentity.ProviderId,
            Identifier = authIdentity.Identifier,
            PasswordHash = authIdentity.PasswordHash,
            LastLoginAt = authIdentity.LastLoginAt,
            IsPrimary = authIdentity.IsPrimary
        };
    }
    public static AuthIdentity ToEntity(this UpdateAuthIdentitiesDto authIdentity, int authId)
    {
        return new AuthIdentity()
        {
            AuthId = authId,
            UserId = authIdentity.UserId,
            ProviderId = authIdentity.ProviderId,
            Identifier = authIdentity.Identifier,
            PasswordHash = authIdentity.PasswordHash,
            LastLoginAt = authIdentity.LastLoginAt,
            IsPrimary = authIdentity.IsPrimary
        };
    }
}

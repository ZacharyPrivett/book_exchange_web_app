using System;
using BookExchange.Api.Data;
using BookExchange.Api.UserManagement.UserDtos;
using BookExchange.Api.UserManagement.UserEntities;
using BookExchange.Api.UserManagement.UserMapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;


namespace BookExchange.Api.UserManagement.UserEndpoints;

public static class UserEndpoint
{

    const string GetUserEndpointName = "GetUser";

    public static UserEndpoint MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("users").WithParameterValidation();

        group.MapGet("/")

        return group;
    }

}

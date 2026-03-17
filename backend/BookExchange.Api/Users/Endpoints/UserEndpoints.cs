using BookExchange.Api.Auth.Entities;
using BookExchange.Api.Data;
using BookExchange.Api.Users.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookExchange.Api.Users.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this WebApplication app)
    {
        var userGroup = app.MapGroup("users")
            .WithTags("User")
            .RequireAuthorization(); // All endpoints require authentications

        // Get current user profile
        userGroup.Map("me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .Produces<UserProfileDto>(StatusCodes.Status200OK);

        // Get all users (Admin only)
        userGroup.Map("", GetAllUsers)
            .RequireAuthorization("Admin")
            .WithName("GetAllUsers")
            .Produces<List<UserProfileDto>>(StatusCodes.Status200OK);

        // Delete user (Admin only)
        userGroup.MapDelete("{userId}", DeleteUser)
            .RequireAuthorization("Admin")
            .WithName("DeleteUser")
            .Produces(StatusCodes.Status204NoContent);
        
        return userGroup;
    }

    private static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        UserManager<ApplicationUser> userManager,
        BookExchangeContext context)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound();
        }

        var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            return Results.NotFound(new { message = "User profile not found" });
        }

        var roles = await userManager.GetRolesAsync(user);

        var userDto = new UserProfileDto(
            user.Id,
            user.Email!,
            profile.FirstName,
            profile.LastName,
            profile.PhoneNumber,
            profile.AvatarUrl,
            profile.Age,
            roles.ToList()
        );

        return Results.Ok(userDto);
    }

    private static async Task<IResult> GetAllUsers(
        UserManager<ApplicationUser> userManager,
        BookExchangeContext context)
    {
        var users = userManager.Users.Where(u => u.IsActive).ToList();

        var userDtos = new List<UserProfileDto>();
        foreach (var user in users)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null) continue; // Skips users without profiles

            var roles = await userManager.GetRolesAsync(user);
            userDtos.Add(new UserProfileDto(
                user.Id,
                user.Email!,
                profile.FirstName,
                profile.LastName,
                profile.PhoneNumber,
                profile.AvatarUrl,
                profile.Age,
                roles.ToList()
            ));
        }

        return Results.Ok(userDtos);
    }

    private static async Task<IResult> DeleteUser(
        string userId,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound();
        }    

        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return Results.NoContent();
    }
    
}

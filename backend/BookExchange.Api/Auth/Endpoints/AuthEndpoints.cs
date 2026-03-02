using BookExchange.Api.Auth.Dtos;
using BookExchange.Api.Auth.Entities;
using BookExchange.Api.Auth.Services;
using BookExchange.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BookExchange.Api.Auth.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var authGroup = app.MapGroup("auth").WithTags("Authentication");

        // Registration
        authGroup.MapPost("register", Register)
            .WithName("Register")
            .Produces<AuthResponseDto>(StatusCodes.Status201Created)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);
        
        return authGroup;
    }

    // Register
    private static async Task<IResult> Register(
        RegisterDto dto,
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        IEmailService emailService,  
        IConfiguration configuration,
        BookExchangeContext context)
    {
        // Check if user already exist
        var existingUser = await userManager.FindByEmailAsync(dto.Email);

        if (existingUser != null)
        {
            return Results.BadRequest(new { message = "User with this email already exists"});
        }

        // Check if DisplayName is available
        var existingDisplayName = await context.UserProfiles.AnyAsync(p => p.DisplayName == dto.DisplayName);

        if (existingDisplayName)
        {
            return Results.BadRequest(new { message = "Username is taken"});   
        }

        // Create ApplicationUser for auth
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow 
        };

        var results = await userManager.CreateAsync(user, dto.Password);

        if (!results.Succeeded)
        {
            return Results.BadRequest(new { errors = results.Errors.Select(e => e.Description) });
        }

        // Create UserProfile
        var profile = new UserProfile
        {
            UserId = user.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DisplayName = dto.DisplayName,
            PhoneNumber = dto.PhoneNumber,
            Age = dto.Age,
            CreatedAt = DateTime.UtcNow
        };

        context.UserProfiles.Add(profile);
        await context.SaveChangesAsync();

        // Assign default "User" role
        await userManager.AddToRoleAsync(user, "User");

        // Generate email confirmation token

    }




}

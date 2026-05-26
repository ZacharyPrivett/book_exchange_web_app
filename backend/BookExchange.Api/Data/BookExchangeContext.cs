using System;
using BookExchange.Api.Auth.Entities;
using BookExchange.Api.Books.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Api.Data;

public class BookExchangeContext : IdentityDbContext<ApplicationUser>
{
    public BookExchangeContext(DbContextOptions<BookExchangeContext> options)
        : base(options) {}

    // Book DbSets
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Condition> Condition => Set<Condition>();

    // Auth DbSets
    public DbSet<RefreshToken> RefreshToken => Set<RefreshToken>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {   
        // Configure Identity tables
        base.OnModelCreating(modelBuilder);

        // Genre seed data
        // ToDo: Update once book api is added
        modelBuilder.Entity<Genre>().HasData(
            new { Id = 1, Name = "Fantasy" },
            new { Id = 2, Name = "Science Fiction" },
            new { Id = 3, Name = "Romance" },
            new { Id = 4, Name = "History" },
            new { Id = 5, Name = "Military Thriller" }
        );

        // Book condition seed data
        modelBuilder.Entity<Condition>().HasData(
            new { Id = 1, Name = "New" },
            new { Id = 2, Name = "Like New" },
            new { Id = 3, Name = "Good" },
            new { Id = 4, Name = "Fair" },
            new { Id = 5, Name = "Worn" }
        );

        // Configure UserProfile
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasIndex(e => e.DisplayName).IsUnique();
            entity.HasOne(e => e.User)
                  .WithOne(u => u.Profile)
                  .HasForeignKey<UserProfile>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);    
        });

        // Identiy roles seed data
        var adminRoleId = "1";
        var userRoleId = "2";
        var moderatorRoleId = "3";

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            { 
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "admin-role-v1"
            },
            new IdentityRole
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "user-role-v1"
            },
            new IdentityRole
            {
                Id = moderatorRoleId,
                Name = "Moderator",
                NormalizedName = "MODERATOR",
                ConcurrencyStamp = "moderator-role-v1"
            }
        );
    }
}

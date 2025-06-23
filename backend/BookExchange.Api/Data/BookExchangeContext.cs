using System;
using BookExchange.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Api.Data;

public class BookExchangeContext(DbContextOptions<BookExchangeContext> options)
    : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Condition> Condition => Set<Condition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Genre>().HasData(
            new { Id = 1, Name = "Fantasy" },
            new { Id = 2, Name = "Science Fiction" },
            new { Id = 3, Name = "Romance" },
            new { Id = 4, Name = "History" },
            new { Id = 5, Name = "Military Thriller" }
        );

        modelBuilder.Entity<Condition>().HasData(
            new { Id = 1, Name = "New" },
            new { Id = 2, Name = "Like New" },
            new { Id = 3, Name = "Good" },
            new { Id = 4, Name = "Fair" },
            new { Id = 5, Name = "Worn" }
        );
    }
}

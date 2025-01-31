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
    
}

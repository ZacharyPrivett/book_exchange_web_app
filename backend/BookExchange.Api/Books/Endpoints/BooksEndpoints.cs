using BookExchange.Api.Data;
using BookExchange.Api.Dtos;
using BookExchange.Api.Entities;
using BookExchange.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Api.Endpoints;

public static class BooksEndpoints
{
    const string GetBookEndpointName = "GetBook";

    public static RouteGroupBuilder MapBooksEndpoints(this WebApplication app) 
    {
        var group = app.MapGroup("books").WithParameterValidation();  

        // GET /books
        group.MapGet("/", async (BookExchangeContext dbContext) =>
            await dbContext.Books
                    .Include(book => book.Genre)
                    .Include(book => book.Condition)
                    .Select(book => book.ToBookSummaryDto())
                    .AsNoTracking()
                    .ToListAsync());

        // GET /books/1
        group.MapGet("/{id}", async (int id, BookExchangeContext dbContext) =>
        {
            Book? book = await dbContext.Books.FindAsync(id);

            return book is null ? Results.NotFound() :
                Results.Ok(book.ToBookDetailsDto());
        })
        .WithName(GetBookEndpointName);

        // POST /books
        group.MapPost("/", async (CreateBookDto newBook, BookExchangeContext dbContext) =>
        {
            Book book = newBook.ToEntity();

            dbContext.Books.Add(book);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(
                GetBookEndpointName,
                new { id = book.Id },
                book.ToBookDetailsDto());
        });

        // PUT /books
        group.MapPut("/{id}", async (int id, UpdateBookDto updatedBook, BookExchangeContext dbContext) => 
        {
            var existingBook = await dbContext.Books.FindAsync(id);

            if (existingBook is null)
            {
                return Results.NotFound();
            }

            dbContext.Entry(existingBook).CurrentValues.SetValues(updatedBook.ToEntity(id));

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE /book/1
        group.MapDelete("/{id}", async (int id, BookExchangeContext dbContext) => 
        {
            await dbContext.Books.Where(book => book.Id == id).ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}

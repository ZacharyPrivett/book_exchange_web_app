using BookExchange.Api.Data;
using BookExchange.Api.Dtos;
using BookExchange.Api.Entities;
using BookExchange.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Api.Endpoints;

public static class BooksEndpoints
{
    const string GetBookEndpointName = "GetBook";

    private static readonly List<BookSummaryDto> books = [
        new (
            1, 
            "Without Remorse", 
            "Tom Clancy", 
            "Mility Thriller", 
            "970-0-425-14332-2", 
            "Ok", 
            "Military/Spy thriller. Action",
            780
        ),
        new (
            2, 
            "The Way of Kings", 
            "Brandon Sanderson", 
            "Epic Fantasy", 
            "670-0-323-17892-1", 
            "Good", 
            "Fantasy World. Magic. Epic quests",
            1120
        ),
        new (
            3, 
            "The Name of the Wind", 
            "Patrick Rothfuss", 
            "Epic Fantasy", 
            "795-0-465-19212-5", 
            "Good", 
            "Follows Kvothe, an inkeeper who narates his epic conquest through the world",
            850
        )
    ];

    public static RouteGroupBuilder MapBooksEndpoints(this WebApplication app) 
    {
        var group = app.MapGroup("books").WithParameterValidation();  

        // GET /books
        group.MapGet("/", (BookExchangeContext dbContext) =>
            dbContext.Books
                    .Include(book => book.Genre)
                    .Include(book => book.Condition)
                    .Select(book => book.ToBookSummaryDto())
                    .AsNoTracking());

        // GET /books/1
        group.MapGet("/{id}", (int id, BookExchangeContext dbContext) =>
        {
            Book? book = dbContext.Books.Find(id);

            return book is null ? Results.NotFound() :
                Results.Ok(book.ToBookDetailsDto());
        })
        .WithName(GetBookEndpointName);

        // POST /books
        group.MapPost("/", (CreateBookDto newBook, BookExchangeContext dbContext) =>
        {
            Book book = newBook.ToEntity();

            dbContext.Books.Add(book);
            dbContext.SaveChanges();

            return Results.CreatedAtRoute(
                GetBookEndpointName,
                new { id = book.Id },
                book.ToBookDetailsDto());
        });

        // PUT /books
        group.MapPut("/{id}", (int id, UpdateBookDto updatedBook, BookExchangeContext dbContext) => 
        {
            var existingBook = dbContext.Books.Find(id);

            if (existingBook is null)
            {
                return Results.NotFound();
            }

            dbContext.Entry(existingBook).CurrentValues.SetValues(updatedBook.ToEntity(id));

            dbContext.SaveChanges();

            return Results.NoContent();
        });

        // DELETE /book/1
        group.MapDelete("/{id}", (int id, BookExchangeContext dbContext) => 
        {
            dbContext.Books.Where(book => book.Id == id).ExecuteDelete();

            return Results.NoContent();
        });

        return group;
    }
}

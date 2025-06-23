using BookExchange.Api.Data;
using BookExchange.Api.Dtos;
using BookExchange.Api.Entities;
using BookExchange.Api.Mapping;

namespace BookExchange.Api.Endpoints;

public static class BooksEndpoints
{
    const string GetBookEndpointName = "GetBook";

    private static readonly List<BookDto> books = [
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
        group.MapGet("/", () => books);

        // GET /books/1
        group.MapGet("/{id}", (int id) =>
        {
            BookDto? book = books.Find(book => book.Id == id);

            return book is null ? Results.NotFound() : Results.Ok(book);
        })
        .WithName(GetBookEndpointName);

        // POST /books
        group.MapPost("/", (CreateBookDto newBook, BookExchangeContext dbContext) =>
        {
            Book book = newBook.ToEntity();
            book.Genre = dbContext.Genres.Find(newBook.GenreId);
            book.Condition = dbContext.Condition.Find(newBook.ConditionId);

            dbContext.Books.Add(book);
            dbContext.SaveChanges();

            BookDto bookDto = new(
                book.Id,
                book.Title,
                book.Author,
                book.Genre!.Name,
                book.ISBN,
                book.Condition!.Name,
                book.Description,
                book.Length
            );

            return Results.CreatedAtRoute(
                GetBookEndpointName,
                new { id = book.Id },
                book.ToDto());
        });

        // PUT /books
        group.MapPut("/{id}", (int id, UpdateBookDto updateBook) => 
        {
            var index = books.FindIndex(book => book.Id == id);

            if (index == -1)
            {
                return Results.NotFound();
            }

            books[index] = new BookDto 
            (
                id,
                updateBook.Author,
                updateBook.Title,
                updateBook.Genre,
                updateBook.ISBN,
                updateBook.Condition,
                updateBook.Description,
                updateBook.Length
            );
            return Results.NoContent();
        });

        // DELETE /book/1
        group.MapDelete("/{id}", (int id) => 
        {
            books.RemoveAll(book => book.Id == id);

            return Results.NoContent();
        });

        return group;
    }
}

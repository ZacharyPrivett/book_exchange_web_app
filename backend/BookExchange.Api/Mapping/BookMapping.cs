using BookExchange.Api.Dtos;
using BookExchange.Api.Entities;

namespace BookExchange.Api.Mapping;

public static class BookMapping
{
    public static Book ToEntity(this CreateBookDto book)
    {
        return new Book()
        {
            Title = book.Title,
            Author = book.Author,
            GenreId = book.GenreId,
            ISBN = book.ISBN,
            ConditionId = book.ConditionId,
            Description = book.Description,
            Length = book.Length
        };
    }

    public static BookDto ToDto(this Book book)
    {
            return new(
            book.Id,
            book.Title,
            book.Author,
            book.Genre!.Name,
            book.ISBN,
            book.Condition!.Name,
            book.Description,
            book.Length
        );
    }
}


namespace BookExchange.Api.Books.Dtos;

public enum BookSearchResultSource
{
    Local = 0,
    GoogleBooks = 1,
    Manual = 2
}

public record class BookSearchResultDto(
    int? CatalogId,
    string? GoogleBooksId,
    string Title,
    List<string> Authors,
    List<string> Categories,
    string? Publisher,
    int? PublishedYear,
    int? PageCount,
    string? CoverImageUrl,
    string? Description,
    string? Isbn10,
    string? Isbn13,
    BookSearchResultSource Source
);
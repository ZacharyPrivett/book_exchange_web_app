using BookExchange.Api.Books.Dtos;

namespace BookExchange.Api.Books.Services;

public interface IGoogleBooksService
{
    Task<IReadOnlyList<BookSearchResultDto>> SearchAsync(string query, int maxResults = 10);
    Task<BookSearchResultDto?> GetByIsbnAsync(string isbn);
    Task<BookSearchResultDto?> GetByIdAsync(string googleBooksId);
}

namespace BookExchange.Api.Books.Entities;

public class BookCatalog
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? ISBN13 { get; set; }
    public string? ISBN10 { get; set; } 
    public string? Publisher { get; set; }
    public int? PublishedYear { get; set; }
    public int? PageCount { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }

     // Cache key so the same Google Books volume is never re-fetched/re-created
    public required string? GoogleBooksId { get; set; }

    public ICollection<Author> Authors { get; set; } = [];
    public ICollection<Genre> Genres { get; set; } = [];
    public ICollection<Listing> Listings { get; set; } = [];
}
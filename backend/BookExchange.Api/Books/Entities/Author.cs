namespace BookExchange.Api.Books.Entities;

public class Author
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<BookCatalog> Books { get; set; } = [];
}
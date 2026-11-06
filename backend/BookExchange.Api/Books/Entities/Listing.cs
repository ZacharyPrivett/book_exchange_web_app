
using BookExchange.Api.Auth.Entities;

namespace BookExchange.Api.Books.Entities;


public class Listing
{
    public int Id { get; set; }

    public int BookCatalogId { get; set; }
    public BookCatalog? BookCatalog { get; set; }

    public required string OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }

    public int ConditionId { get; set; }
    public Condition? Condition { get; set; }

    public string? Notes { get; set; }

    public ListingStatus Status { get; set; } = ListingStatus.Available;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

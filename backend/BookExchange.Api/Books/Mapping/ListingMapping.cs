using BookExchange.Api.Books.Dtos;
using BookExchange.Api.Books.Entities;

namespace BookExchange.Api.Books.Mapping;

public static class ListingMapping
{
    public static ListingSummaryDto ToSummaryDto(this Listing listing) =>
        new(
            listing.Id,
            listing.BookCatalog!.Title,
            listing.BookCatalog.Authors.Select(a => a.Name).ToList(),
            listing.BookCatalog.CoverImageUrl,
            listing.Condition!.Name,
            listing.Status.ToString());

    // currentUserId is the caller's user ID (null for anonymous requests), used to compute
    // IsOwner so the frontend knows whether to show owner-only actions (edit notes, delist, etc.).
    public static ListingDetailsDto ToDetailsDto(this Listing listing, string? currentUserId = null) =>
        new(
            listing.Id,
            listing.BookCatalog!.Title,
            listing.BookCatalog.Authors.Select(a => a.Name).ToList(),
            listing.BookCatalog.Genres.Select(g => g.Name).ToList(),
            listing.BookCatalog.Publisher,
            listing.BookCatalog.PublishedYear,
            listing.BookCatalog.PageCount,
            listing.BookCatalog.CoverImageUrl,
            listing.BookCatalog.Description,
            listing.Condition!.Name,
            listing.Status.ToString(),
            listing.Notes,
            listing.Owner?.Profile?.DisplayName ?? listing.Owner?.UserName ?? "Unknown",
            listing.CreatedAt,
            currentUserId is not null && currentUserId == listing.OwnerId);
}

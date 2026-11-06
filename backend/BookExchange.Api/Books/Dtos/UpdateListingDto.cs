using BookExchange.Api.Books.Entities;

namespace BookExchange.Api.Books.Dtos;

// DTO for updating a book listing's status, condition, and notes.
public record class UpdateListingDto(
    ListingStatus? Status,
    int? ConditionId,
    string? Notes
);

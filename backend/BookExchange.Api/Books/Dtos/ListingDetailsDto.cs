namespace BookExchange.Api.Books.Dtos;

public record class ListingDetailsDto(
    int Id,
    string Title,
    List<string> Authors,
    List<string> Genres,
    string? Publisher,
    int? PublishedYear,
    int? PageCount,
    string? CoverImageUrl,
    string? Description,
    string ConditionName,
    string Status,
    string? Notes,
    string OwnerDisplayName,
    DateTime CreatedAt
);

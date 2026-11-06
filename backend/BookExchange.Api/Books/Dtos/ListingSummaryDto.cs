namespace BookExchange.Api.Books.Dtos;

public record class ListingSummaryDto(
    int Id,
    string Title,
    List<string> Authors,
    string? CoverImageUrl,
    string ConditionName,
    string Status
);

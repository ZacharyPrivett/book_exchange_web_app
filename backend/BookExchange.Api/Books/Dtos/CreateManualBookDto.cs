using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Books.Dtos;

public record class CreateManualBookDto(
    [Required] string Title,
    [Required] List<string> Authors,
    List<string>? Genres,
    string? Publisher,
    int? PublishedYear,
    int? PageCount,
    string? CoverImageUrl,
    string? Description,
    string? Isbn10,
    string? Isbn13,
    [Required] int ConditionId,
    string? Notes
);

using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Books.Dtos;

public record class CreateListingDto(
    int? CatalogId,
    string? GoogleBooksId,
    [Required] int ConditionId,
    string? Notes
);

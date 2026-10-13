using System;
using BookExchange.Api.Books.Dtos;
using BookExchange.Api.Books.Entities;

namespace BookExchange.Api.Books.Mapping;

public static class BookCatalogMapping
{
    public static BookSearchResultDto ToSearchResultDto(this BookCatalog catalog) =>
        new(
            catalog.Id,
            catalog.GoogleBooksId,
            catalog.Title,
            catalog.Authors.Select(a => a.Name).ToList(),
            catalog.Genres.Select(g => g.Name).ToList(),
            catalog.Publisher,
            catalog.PublishedYear,
            catalog.PageCount,
            catalog.CoverImageUrl,
            catalog.Description,
            catalog.ISBN10,
            catalog.ISBN13,
            catalog.GoogleBooksId is null ? BookSearchResultSource.Manual : BookSearchResultSource.Local);
}

using BookExchange.Api.Books.Entities;
using BookExchange.Api.Books.Dtos;


namespace BookExchange.Api.Books.Mapping;

public static class GenreMapping
{
    public static GenreDto ToDto(this Genre genre)
    {
        return new GenreDto(genre.Id, genre.Name);
    }
}

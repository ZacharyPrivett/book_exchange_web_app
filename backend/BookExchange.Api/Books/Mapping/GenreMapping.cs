using System;
using BookExchange.Api.Entities;
using BookExchange.Api.Dtos;


namespace BookExchange.Api.Mapping;

public static class GenreMapping
{
    public static GenreDto ToDto(this Genre genre)
    {
        return new GenreDto(genre.Id, genre.Name);
    }
}

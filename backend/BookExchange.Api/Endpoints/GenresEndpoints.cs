using System;
using BookExchange.Api.Data;
using BookExchange.Api.Entities;
using BookExchange.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Api.Endpoints;

public static class GenresEndpoints
{
    public static RouteGroupBuilder MapGenresEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("genres");

        group.MapGet("/", async (BookExchangeContext dbContext) =>
            await dbContext.Genres.Select(genre => genre.ToDto()).AsNoTracking().ToListAsync());

        return group;
    }
}

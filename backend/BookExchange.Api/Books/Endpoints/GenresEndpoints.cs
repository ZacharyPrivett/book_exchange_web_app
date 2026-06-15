using BookExchange.Api.Data;
using BookExchange.Api.Books.Entities;
using BookExchange.Api.Books.Mapping;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Api.Books.Endpoints;

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

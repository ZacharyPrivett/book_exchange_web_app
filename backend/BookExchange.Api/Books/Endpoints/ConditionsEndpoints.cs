using System;
using BookExchange.Api.Data;
using BookExchange.Api.Books.Entities;
using BookExchange.Api.Books.Mapping;
using Microsoft.EntityFrameworkCore;



namespace BookExchange.Api.Books.Endpoints;

public static class ConditionsEndpoints
{
    public static RouteGroupBuilder MapConditionsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("conditions");

        group.MapGet("/", async (BookExchangeContext dbContext) =>
        await dbContext.Condition.Select(condition => condition.ToDto()).AsNoTracking().ToListAsync());

        return group;
    }
}

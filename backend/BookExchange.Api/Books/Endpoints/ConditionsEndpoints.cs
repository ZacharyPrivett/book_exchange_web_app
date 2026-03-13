using System;
using BookExchange.Api.Data;
using BookExchange.Api.Entities;
using BookExchange.Api.Mapping;
using Microsoft.EntityFrameworkCore;



namespace BookExchange.Api.Endpoints;

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

using System;

namespace BookExchange.Api.User.UserEntities;

public class Provider
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

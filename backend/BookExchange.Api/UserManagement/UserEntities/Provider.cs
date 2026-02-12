using System;

namespace BookExchange.Api.UserManagement.UserEntities;

public class Provider
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

using System;

namespace BookExchange.Api.Entities;

public class Condition
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

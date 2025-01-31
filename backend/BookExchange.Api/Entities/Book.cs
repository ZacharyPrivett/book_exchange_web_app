using System;

namespace BookExchange.Api.Entities;

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string ISBN { get; set; }
    public int GenreId { get; set; }
    public Genre? Genre { get; set; }
    public int ConditionId { get; set; }
    public Condition? Condition { get; set; }
    public required string Description { get; set; }
    public required int Length { get; set; }
}

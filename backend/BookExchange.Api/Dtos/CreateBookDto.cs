using System.ComponentModel.DataAnnotations;
using BookExchange.Api.Entities;

namespace BookExchange.Api.Dtos;

public record class CreateBookDto
(
    [Required][StringLength(50)] string Title, 
    [Required][StringLength(50)] string Author, 
    int GenreId, 
    [Required] string ISBN, 
    int ConditionId, 
    [Required] string Description,
    [Required] int Length
);


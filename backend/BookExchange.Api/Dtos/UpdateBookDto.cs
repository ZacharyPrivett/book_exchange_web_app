using System.ComponentModel.DataAnnotations;

namespace BookExchange.Api.Dtos;

public record class UpdateBookDto
(
    [Required][StringLength(50)] string Title, 
    [Required][StringLength(50)] string Author, 
    [Required][StringLength(50)] string Genre, 
    [Required] string ISBN, 
    [Required] string Condition, 
    [Required] string Description,
    [Required] int Length
);

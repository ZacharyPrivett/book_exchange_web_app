namespace BookExchange.Api.Dtos;

public record class BookDetailsDto
(
    int Id, 
    string Title, 
    string Author, 
    int GenreId, 
    string ISBN, 
    int ConditionId, 
    string Description,
    int Length
);

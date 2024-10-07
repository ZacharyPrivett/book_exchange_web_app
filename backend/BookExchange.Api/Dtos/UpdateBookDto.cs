namespace BookExchange.Api.Dtos;

public record class UpdateBookDto
(
    string Title, 
    string Author, 
    string Genre, 
    string ISBN, 
    string Condition, 
    string Description,
    int Length
);

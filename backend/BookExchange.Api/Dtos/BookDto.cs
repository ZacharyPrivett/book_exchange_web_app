namespace BookExchange.Api.Dtos;

public record class BookDto
(
    int Id, 
    string Title, 
    string Author, 
    string Genre, 
    string ISBN, 
    string Condition, 
    string Description,
    int Length
);

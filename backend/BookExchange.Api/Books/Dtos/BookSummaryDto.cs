namespace BookExchange.Api.Books.Dtos;

public record class BookSummaryDto
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

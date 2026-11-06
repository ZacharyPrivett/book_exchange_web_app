-- Connect to: localhost\SQLEXPRESS
-- Database: BookExchange

-- View all users
SELECT * FROM AspNetUsers;

-- View user profiles
SELECT * FROM UserProfiles;

SELECT * FROM BookCatalogs;

SELECT * FROM Listings;


-- View all tables
-- SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- View users books
SELECT u.Id AS UserId, u.UserName, l.Id AS ListingId, l.Status, b.Title, b.ISBN10, b.ISBN13
FROM AspNetUsers u
JOIN Listings l ON u.Id = l.OwnerId
JOIN BookCatalogs b ON l.BookCatalogId = b.Id;

SELECT * FROM Genres;

SELECT
    bc.Id,
    bc.Title,
    STRING_AGG(g.Name, ', ') WITHIN GROUP (ORDER BY g.Name) AS Genres
FROM BookCatalogs bc
LEFT JOIN BookCatalogGenres bcg ON bcg.BooksId = bc.Id
LEFT JOIN Genres g ON g.Id = bcg.GenresId
GROUP BY bc.Id, bc.Title
ORDER BY bc.Title;
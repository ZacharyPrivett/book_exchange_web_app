-- Connect to: localhost\SQLEXPRESS
-- Database: BookExchange

-- View all users
SELECT * FROM AspNetUsers;

-- View user profiles
SELECT * FROM UserProfiles;

-- View all tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

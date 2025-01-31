using BookExchange.Api.Data;
using BookExchange.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connString = "Data Source=BookExchange.db";
builder.Services.AddSqlite<BookExchangeContext>(connString);

var app = builder.Build();

app.MapBooksEndpoints();

app.Run();

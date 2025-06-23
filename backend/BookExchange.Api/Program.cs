using BookExchange.Api.Data;
using BookExchange.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration.GetConnectionString("BookExchange");
builder.Services.AddSqlite<BookExchangeContext>(connString);

var app = builder.Build();

app.MapBooksEndpoints();

app.MigrateDb();

app.Run();

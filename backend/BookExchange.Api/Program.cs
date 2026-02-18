using BookExchange.Api.Data;
using BookExchange.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;


var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration.GetConnectionString("BookExchange");
builder.Services.AddSqlite<BookExchangeContext>(connString);

builder.Services.AddCors();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapBooksEndpoints();
app.MapGenresEndpoints();
app.MapConditionsEndpoints();

await app.MigrateDbAsync();

app.Run();

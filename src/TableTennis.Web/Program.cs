using Microsoft.EntityFrameworkCore;
using TableTennis.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TableTennisDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TableTennisDbContext")));

var app = builder.Build();

app.MapGet("/", () => "TableTennis: Lab 2 (Code First, EF Core, PostgreSQL)");

app.Run();

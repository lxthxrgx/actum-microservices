using Counterparty.service;
using Microsoft.EntityFrameworkCore;
using SharedLibraries.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseModel>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<CounterpartyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
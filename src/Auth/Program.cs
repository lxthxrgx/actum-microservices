using Microsoft.EntityFrameworkCore;
using SharedLibraries.Database;
using Scalar.AspNetCore;
using Auth.service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseModel>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging(false);
    options.UseLoggerFactory(LoggerFactory.Create(builder => builder
        .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
        .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)));
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<SignIn>();
builder.Services.AddScoped<SignUp>();
builder.Services.AddScoped<jwtService>();

builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection("JWT")
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
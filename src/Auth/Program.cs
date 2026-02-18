using Microsoft.EntityFrameworkCore;
using SharedLibraries.Database;
using Scalar.AspNetCore;
using Auth.service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
builder.Services.AddScoped<httpOnly>();

builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection("JWT")
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:KEY"])),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:ISSUER"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:AUDIENCE"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Read JWT from HttpOnly cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Check if token is in cookie
            if (context.Request.Cookies.ContainsKey("accessToken"))
            {
                context.Token = context.Request.Cookies["accessToken"];
            }
            return Task.CompletedTask;
        }
    };
});

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
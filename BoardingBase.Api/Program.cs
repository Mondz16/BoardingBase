using BoardingBase.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<BoardingBaseDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddHealthChecks().AddRedis(builder.Configuration.GetSection("Redis:ConnectionString").Value);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:5173", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.MapHealthChecks("/health");
app.MapGet("/test", () =>
{
    return Results.Ok(new
    {
        message = "Testing!"
    });
});

app.Run();

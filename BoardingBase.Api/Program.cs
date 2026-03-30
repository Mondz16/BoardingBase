using System.Text;
using BoardingBase.Api.Endpoints;
using BoardingBase.Application.Common;
using BoardingBase.Application.Interfaces;
using BoardingBase.Infrastructure.Identity;
using BoardingBase.Infrastructure.Persistence;
using BoardingBase.Infrastructure.Service;
using BoardingBase.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<BoardingBaseDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var jwtSetting = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,

        ValidIssuer = jwtSetting.Issuer,
        ValidAudience = jwtSetting.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SigningKey))
    };
});
builder.Services.AddAuthorization();


var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    if (app.Environment.IsDevelopment())
    {
        await IdentitySeeder.SeedRolesAsync(roleManager);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.MapHealthChecks("/health");
app.MapAuthEndpoints();

app.Run();
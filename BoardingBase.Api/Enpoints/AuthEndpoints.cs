using System;
using System.Security.Claims;
using BoardingBase.Application.DTOs.Auth;
using BoardingBase.Application.Interfaces;

namespace BoardingBase.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", async (IAuthService authService, HttpResponse response, RegisterRequest request) =>
        {
            var result = await authService.RegisterAsync(request);

            response.Cookies.Append("refreshToken", "...", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Results.Ok(result);
        });

        group.MapPost("/login", async (IAuthService authService, HttpResponse response, LoginRequest request) =>
        {
            var result = await authService.LoginAsync(request);

            response.Cookies.Append("refreshToken", "...", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Results.Ok(result);
        });

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            return Results.Ok(new
            {
                userId = user.FindFirstValue(ClaimTypes.NameIdentifier),
                email = user.FindFirstValue(ClaimTypes.Email)
            });
        }).RequireAuthorization();
    }
}

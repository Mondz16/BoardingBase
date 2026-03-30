using System;
using System.Security.Claims;
using BoardingBase.Api.Utils;
using BoardingBase.Application.DTOs.Auth;
using BoardingBase.Application.Interfaces;
using BoardingBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BoardingBase.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/auth");

        group.MapPost("/register", async (IAuthService authService, HttpResponse response, RegisterRequest request) =>
        {
            var result = await authService.RegisterAsync(request);

            response.Cookies.Append("refreshToken", "...", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Results.Ok(new  ApiResponse(200, "Register sucessfully!", result));
        })
        .WithTags("Auth")
        .WithDisplayName("Register")
        .WithSummary("Register the users");

        group.MapPost("/login", async (IAuthService authService, HttpResponse response, LoginRequest request) =>
        {
            var result = await authService.LoginAsync(request);

            response.Cookies.Append("refreshToken", "...", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Results.Ok(new  ApiResponse(200, "Login sucessfully!", result));
        })
        .WithTags("Auth")
        .WithDisplayName("Login")
        .WithSummary("Login the users");

        group.MapGet("/me", async (UserManager<AppUser> userManager, ClaimsPrincipal user, string fullName) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var appUser = await userManager.FindByIdAsync(userId!);

            if(appUser == null)
                return Results.NotFound();

            appUser.FullName = fullName;

            await userManager.UpdateAsync(appUser);

            return Results.Ok(new ApiResponse(
                200,
                "Profile Updated!",
                appUser
            ));
        })
        .WithTags("Auth")
        .WithDisplayName("Me")
        .WithSummary("Check the user identity by passing access token")
        .RequireAuthorization();
        
        group.MapGet("/refresh", async (IAuthService authService, HttpRequest request) =>
        {
            var refreshToken = request.Cookies["refreshToken"];

            if(string.IsNullOrEmpty(refreshToken))
                return Results.Unauthorized();

            var result = await authService.RefreshAsync(refreshToken);
            
            return Results.Ok(new ApiResponse(200, "Token Refresh!", result));
        })
        .WithTags("Auth")
        .WithDisplayName("Refresh Token")
        .WithSummary("Refresh the token of the users");

        group.MapPost("/logout", async (IAuthService authService, HttpRequest request, HttpResponse response) =>
        {
            var refreshToken = request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await authService.LogoutAsync(refreshToken);
            }

            response.Cookies.Delete("refreshToken");

            return Results.Ok(new ApiResponse(200, "User logged out!"));
        })
        .WithTags("Auth")
        .WithDisplayName("Logout")
        .WithSummary("Logout the user");

        group.MapPost("/forgot-password", async (UserManager<AppUser> userManager, string email) =>
        {
            var user = await userManager.FindByEmailAsync(email);
            if(user == null)
                return Results.NotFound();
            
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            return Results.Ok(new ApiResponse(200, "Generated Password Reset Token!", new {resetToken = token}));
        })
        .WithTags("Auth")
        .WithDisplayName("Forgot Password")
        .WithSummary("Forgot password of the user");

        group.MapPost("/reset-password", async (UserManager<AppUser> userManager, string email, string newPassword, string token) =>
        {
            var user = await userManager.FindByEmailAsync(email);
            if(user == null)
                return Results.NotFound();
            
            var result = await userManager.ResetPasswordAsync(user, token, newPassword);

            if(!result.Succeeded)
                return Results.BadRequest();

            return Results.Ok(new ApiResponse(200, "Reset Password Successfully!", result));
        })
        .WithTags("Auth")
        .WithDisplayName("Reset Password")
        .WithSummary("Reset password of the user");

        group.MapPost("/invite-tenant", async (UserManager<AppUser> userManager, string email) =>
        {
            var inviteToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            return Results.Ok( new ApiResponse(
                200, 
                "Invite Token Sent!", 
                new
                {
                    inviteLink = $"http://localhost:5112/v1/api/auth/register?token={inviteToken}&email{email}"
                }
            ));
        })
        .WithTags("Auth")
        .WithDisplayName("Invite Tenant")
        .WithSummary("Send invite link to the tenants!");
    }
}

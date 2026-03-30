using System;
using BoardingBase.Application.DTOs.Auth;
using BoardingBase.Application.Interfaces;
using BoardingBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;

namespace BoardingBase.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConnectionMultiplexer _redis;

    public AuthService(UserManager<AppUser> userManager, ITokenService tokenService, IConnectionMultiplexer redis)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _redis = redis;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var user = new AppUser
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception(errors);
        }
        
        await _userManager.AddToRoleAsync(user, "Landlord");

        var roles = await _userManager.GetRolesAsync(user);

        var authUser = new AuthUser()
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName
        };
        var accessToken = await _tokenService.CreateAccessToken(authUser, roles);

        var refreshToken = _tokenService.CreateRefreshToken();

        await SaveRefreshToken(user.Id, refreshToken);

        return new AuthResponse(accessToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if(user == null)
            throw new Exception("Invalid Credentials!");

        var valid = await _userManager.CheckPasswordAsync(user, request.Password);
        if(!valid)
            throw new Exception("Invalid Credentials!");

        var roles = await _userManager.GetRolesAsync(user);
        var authUser = new AuthUser
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName
        };
        var accessToken = await _tokenService.CreateAccessToken(authUser, roles);
        var refreshToken = _tokenService.CreateRefreshToken();

        await SaveRefreshToken(user.Id, refreshToken);

        return new AuthResponse(accessToken);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var db = _redis.GetDatabase();

        var userId = await db.StringGetAsync(refreshToken);
        if(userId.IsNullOrEmpty)
            throw new Exception("Invalid Refresh Token!");

        var user = await _userManager.FindByIdAsync(userId!);
        var roles = await _userManager.GetRolesAsync(user!);

        var authUser = new AuthUser()
        {
            Id = user!.Id,
            Email = user.Email,
            FullName = user.FullName
        };
        var newAccessToken = await _tokenService.CreateAccessToken(authUser!, roles);

        return new AuthResponse(newAccessToken);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(refreshToken);
    }

    private async Task SaveRefreshToken(Guid userId, string refreshToken)
    {
        var db = _redis.GetDatabase();

        await db.StringSetAsync(
            refreshToken,
            userId.ToString(),
            TimeSpan.FromDays(7)
        );
    }
}

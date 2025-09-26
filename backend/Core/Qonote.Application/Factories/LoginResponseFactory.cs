using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Factories;

public sealed class LoginResponseFactory : ILoginResponseFactory
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginResponseFactory(ITokenService tokenService, UserManager<ApplicationUser> userManager)
    {
        _tokenService = tokenService;
        _userManager = userManager;
    }

    public async Task<LoginResponseDto> CreateAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, accessTokenExpiry) = _tokenService.CreateAccessToken(user, roles);
        var (refreshToken, refreshTokenExpiry) = _tokenService.CreateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiry;
        await _userManager.UpdateAsync(user);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiry,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = user.RefreshTokenExpiryTime!.Value
        };
    }
}

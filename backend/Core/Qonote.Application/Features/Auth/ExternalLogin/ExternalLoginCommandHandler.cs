using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.ExternalLogin;

public sealed class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, LoginResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IGoogleAuthService _googleAuthService;

    public ExternalLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IGoogleAuthService googleAuthService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _googleAuthService = googleAuthService;
    }

    public async Task<LoginResponseDto> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        if (request.Provider.ToLower() != "google")
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Provider", "Only Google provider is supported.") });
        }

        const string redirectUri = "https://developers.google.com/oauthplayground";
        
        var userInfo = await _googleAuthService.ExchangeCodeForUserInfoAsync(request.Code, redirectUri);

        var user = await _userManager.FindByEmailAsync(userInfo.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = userInfo.Email,
                UserName = userInfo.Email,
                Name = userInfo.FirstName ?? string.Empty,
                Surname = userInfo.LastName ?? string.Empty,
                ProfileImageUrl = userInfo.ProfilePictureUrl,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new ValidationException(result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));

            await _userManager.AddToRoleAsync(user, "FreeUser");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, accessExpiry) = _tokenService.CreateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiry,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = user.RefreshTokenExpiryTime!.Value
        };
    }
}
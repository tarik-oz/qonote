using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Auth.Login", "Invalid email or password.") });
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Auth.Login", "Invalid email or password.") });
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

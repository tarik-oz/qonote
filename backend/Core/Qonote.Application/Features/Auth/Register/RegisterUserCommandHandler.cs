using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, LoginResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Name = request.Name,
            Surname = request.Surname,
            Email = request.Email,
            UserName = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));
        }

        await _userManager.AddToRoleAsync(user, "FreeUser");

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

using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponseDto>
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RefreshTokenCommandHandler(ITokenService tokenService, UserManager<ApplicationUser> userManager)
    {
        _tokenService = tokenService;
        _userManager = userManager;
    }

    public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredAccessToken(request.AccessToken);
        if (principal is null)
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("AccessToken", "Geçersiz erişim token'ı.") });
        }

    var userId = principal.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("AccessToken", "Token kullanıcı bilgisi içermiyor.") });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("RefreshToken", "Geçersiz veya süresi dolmuş yenileme token'ı.") });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (newAccess, accessExpiry) = _tokenService.CreateAccessToken(user, roles);
        var newRefresh = _tokenService.GenerateRefreshToken();
        user.RefreshToken = newRefresh;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new LoginResponseDto
        {
            AccessToken = newAccess,
            AccessTokenExpiresAt = accessExpiry,
            RefreshToken = newRefresh,
            RefreshTokenExpiresAt = user.RefreshTokenExpiryTime!.Value
        };
    }
}

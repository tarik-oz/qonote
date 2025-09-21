using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponseDto>
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoginResponseFactory _loginResponseFactory;

    public RefreshTokenCommandHandler(ITokenService tokenService, UserManager<ApplicationUser> userManager, ILoginResponseFactory loginResponseFactory)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _loginResponseFactory = loginResponseFactory;
    }

    public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        ClaimsPrincipal? principal;
        try
        {
            principal = _tokenService.GetPrincipalFromExpiredAccessToken(request.AccessToken);
        }
        catch (SecurityTokenException)
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Token", "Invalid Access Token.") });
        }

        if (principal?.Claims is null)
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Token", "Invalid Access Token.") });
        }

        var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Token", "Invalid Access Token. User identifier not found.") });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Token", "Invalid Refresh Token.") });
        }

        return await _loginResponseFactory.CreateAsync(user);
    }
}

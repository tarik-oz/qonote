using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(ITokenService tokenService, UserManager<ApplicationUser> userManager, ILoginResponseFactory loginResponseFactory, ILogger<RefreshTokenCommandHandler> logger)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _loginResponseFactory = loginResponseFactory;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        ClaimsPrincipal? principal;
        var accessToken = request.AccessToken?.Trim();
        var refreshToken = request.RefreshToken?.Trim();

        try
        {
            principal = _tokenService.GetPrincipalFromExpiredAccessToken(accessToken!);
        }
        catch (SecurityTokenException)
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Token", "Invalid Access Token.")]);
        }

        if (principal?.Claims is null)
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Token", "Invalid Access Token.")]);
        }

        var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Token", "Invalid Access Token. User identifier not found.")]);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Token", "Invalid Refresh Token.")]);
        }

        var response = await _loginResponseFactory.CreateAsync(user);
        _logger.LogInformation("Refresh token exchange successful. {UserId}", user.Id);
        return response;
    }
}

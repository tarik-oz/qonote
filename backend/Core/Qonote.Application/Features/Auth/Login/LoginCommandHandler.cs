using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoginResponseFactory _loginResponseFactory;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(UserManager<ApplicationUser> userManager, ILoginResponseFactory loginResponseFactory, ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _loginResponseFactory = loginResponseFactory;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // All business rules (user existence, password validity, email confirmation) 
        // are handled by the BusinessRulesBehavior.
        var normalizedEmail = request.Email?.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedEmail!);

        var response = await _loginResponseFactory.CreateAsync(user!);

        // PII-safe audit log (prefer userId over email)
        _logger.LogInformation("User logged in. UserId={UserId}", user!.Id);

        return response;
    }
}

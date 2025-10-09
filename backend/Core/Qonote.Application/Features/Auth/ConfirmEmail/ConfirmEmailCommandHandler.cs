using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, LoginResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoginResponseFactory _loginResponseFactory;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(UserManager<ApplicationUser> userManager, ILoginResponseFactory loginResponseFactory, ILogger<ConfirmEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _loginResponseFactory = loginResponseFactory;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        // All rules (user exists, code is valid, code is not expired) are checked by BusinessRulesBehavior.
        // We can safely assume the user exists and the code is valid.
        var email = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        user!.EmailConfirmed = true;
        user.EmailConfirmationCode = null; // Clean up the code
        user.EmailConfirmationCodeExpiry = null; // Clean up the expiry

        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Email confirmed for user {UserId}.", user.Id);

        // Email confirmed, now log the user in by generating tokens
        return await _loginResponseFactory.CreateAsync(user);
    }
}

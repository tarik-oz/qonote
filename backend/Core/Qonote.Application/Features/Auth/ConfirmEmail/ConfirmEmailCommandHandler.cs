using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, LoginResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoginResponseFactory _loginResponseFactory;

    public ConfirmEmailCommandHandler(UserManager<ApplicationUser> userManager, ILoginResponseFactory loginResponseFactory)
    {
        _userManager = userManager;
        _loginResponseFactory = loginResponseFactory;
    }

    public async Task<LoginResponseDto> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        // All rules (user exists, code is valid, code is not expired) are checked by BusinessRulesBehavior.
        // We can safely assume the user exists and the code is valid.
        var user = await _userManager.FindByEmailAsync(request.Email);

        user!.EmailConfirmed = true;
        user.EmailConfirmationCode = null; // Clean up the code
        user.EmailConfirmationCodeExpiry = null; // Clean up the expiry

        await _userManager.UpdateAsync(user);

        // Email confirmed, now log the user in by generating tokens
        return await _loginResponseFactory.CreateAsync(user);
    }
}

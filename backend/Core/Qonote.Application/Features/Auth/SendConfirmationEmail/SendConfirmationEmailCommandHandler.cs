using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Messaging;
using Qonote.Core.Application.Extensions;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.SendConfirmationEmail;

public sealed class SendConfirmationEmailCommandHandler : IRequestHandler<SendConfirmationEmailCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public SendConfirmationEmailCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(SendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        // All rules (user exists, email is not confirmed) are checked by BusinessRulesBehavior.
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Generate code, update user, and get the code back
        var (confirmationCode, _) = await _userManager.GenerateAndSetEmailConfirmationCodeAsync(user!);

        // Send confirmation email
        var emailBody = $"<h1>Welcome to Qonote!</h1><p>Your new confirmation code is: <strong>{confirmationCode}</strong></p>";
        await _emailService.SendEmailAsync(user!.Email!, "Confirm your Qonote Account", emailBody);

        return Unit.Value;
    }
}

using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Messaging;
using Qonote.Core.Application.Extensions;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.SendConfirmationEmail;

public sealed class SendConfirmationEmailCommandHandler : IRequestHandler<SendConfirmationEmailCommand, SendConfirmationEmailResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly ILogger<SendConfirmationEmailCommandHandler> _logger;

    public SendConfirmationEmailCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService, IEmailTemplateRenderer templateRenderer, ILogger<SendConfirmationEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _templateRenderer = templateRenderer;
        _logger = logger;
    }

    public async Task<SendConfirmationEmailResponse> Handle(SendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        // All rules (user exists, email is not confirmed) are checked by BusinessRulesBehavior.
        var normalizedEmail = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        // Generate code, update user, and get the code back
        var (confirmationCode, _) = await _userManager.GenerateAndSetEmailConfirmationCodeAsync(user!);

        // Send confirmation email using template
        var body = await _templateRenderer.RenderAsync(
            templateName: "RegistrationConfirmation",
            placeholders: new Dictionary<string, string>
            {
                ["name"] = user!.Name,
                ["code"] = confirmationCode
            });

        await _emailService.SendEmailAsync(user.Email!, "Confirm your Qonote account", body);

        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Email confirmation code generated. {UserId} {Code}", user.Id, confirmationCode);
        }

        // Success; cooldown remaining is 0 for immediate UI updates
        return new SendConfirmationEmailResponse(CooldownSecondsRemaining: 0);
    }
}

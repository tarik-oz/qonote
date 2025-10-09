using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Messaging;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService, IEmailTemplateRenderer templateRenderer, IConfiguration configuration, ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _templateRenderer = templateRenderer;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        if (user is null)
        {
            return Unit.Value;
        }

        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return Unit.Value;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);

        string? clientBaseUrl = _configuration["Client:BaseUrl"];
        string emailBody;

        if (!string.IsNullOrWhiteSpace(clientBaseUrl))
        {
            var resetUrl = $"{clientBaseUrl.TrimEnd('/')}/reset-password?email={System.Net.WebUtility.UrlEncode(user.Email)}&token={encodedToken}";
            emailBody = await _templateRenderer.RenderAsync(
                templateName: "PasswordReset",
                placeholders: new Dictionary<string, string>
                {
                    ["name"] = user.Name,
                    ["resetUrl"] = resetUrl
                });
        }
        else
        {
            emailBody = await _templateRenderer.RenderAsync(
                templateName: "PasswordResetFallback",
                placeholders: new Dictionary<string, string>
                {
                    ["name"] = user.Name,
                    ["token"] = encodedToken,
                    ["email"] = user.Email ?? string.Empty
                });
        }

        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Password reset token generated. {UserId}", user.Id);
        }

        await _emailService.SendEmailAsync(user.Email!, "Reset your password", emailBody);

        return Unit.Value;
    }
}

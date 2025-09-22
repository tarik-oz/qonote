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
    private readonly IConfiguration _configuration;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService, IConfiguration configuration, ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

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
            emailBody = $"<p>We received a request to reset your password.</p>" +
                        $"<p>If you made this request, click the link below:</p>" +
                        $"<p><a href=\"{resetUrl}\">Reset your password</a></p>";
        }
        else
        {
            emailBody = $"<p>We received a request to reset your password.</p>" +
                        $"<p>If you made this request, use the token below to reset your password:</p>" +
                        $"<p><strong>Token:</strong> {encodedToken}</p>" +
                        $"<p>Email: {System.Net.WebUtility.HtmlEncode(user.Email)}</p>";
        }

        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Password reset token for {Email}: {Token}", user.Email, encodedToken);
        }

        await _emailService.SendEmailAsync(user.Email!, "Reset your password", emailBody);

        return Unit.Value;
    }
}

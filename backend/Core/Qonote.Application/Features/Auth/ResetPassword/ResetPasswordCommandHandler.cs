using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager, ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            // Always return success to avoid account enumeration
            return Unit.Value;
        }

        // If the user does not have a local password (external-only), don't perform reset.
        // This mirrors the ForgotPassword behavior and avoids creating a password where not intended.
        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return Unit.Value;
        }

        // Decode Base64Url token
        string decodedToken;
        try
        {
            var tokenBytes = WebEncoders.Base64UrlDecode(request.Token.Trim());
            decodedToken = Encoding.UTF8.GetString(tokenBytes);
        }
        catch
        {
            // Invalid token format
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Token", "Invalid or expired reset token.")]);
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));
        }

        _logger.LogInformation("Password reset successful. {UserId}", user.Id);

        return Unit.Value;
    }
}

using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.SendConfirmationEmail.Rules;

/// <summary>
/// Prevents resending a confirmation email if the last code was generated less than the cooldown window ago.
/// UX: 30 seconds resend cooldown; the code itself remains valid for 3 minutes elsewhere.
/// </summary>
public sealed class ResendCooldownMustHavePassedRule : IBusinessRule<SendConfirmationEmailCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    // Cooldown window in seconds
    private const int CooldownSeconds = 30;

    public ResendCooldownMustHavePassedRule(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public int Order => 2;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(SendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim();
        var user = await _userManager.FindByEmailAsync(email!);

        if (user is null)
        {
            // Let UserMustExistByEmailRule handle not-found case.
            return Array.Empty<RuleViolation>();
        }

        // If no previous code, allow send
        if (string.IsNullOrWhiteSpace(user.EmailConfirmationCode) || !user.EmailConfirmationCodeExpiry.HasValue)
        {
            return Array.Empty<RuleViolation>();
        }

        // We don't store the creation time, only expiry. Assume default validity window is 3 minutes
        // (from UserManagerExtensions.GenerateAndSetEmailConfirmationCodeAsync default of 3 minutes).
        // Derive approximate createdAt = expiry - 3 minutes; use that to enforce a 30s cooldown.
        var validityWindow = TimeSpan.FromMinutes(3);
        var createdAt = user.EmailConfirmationCodeExpiry.Value - validityWindow;

        var elapsed = DateTime.UtcNow - createdAt;
        var remaining = TimeSpan.FromSeconds(CooldownSeconds) - elapsed;

        if (remaining > TimeSpan.Zero)
        {
            var remainingSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
            return
            [
                new RuleViolation(
                        key: nameof(request.Email),
                        message: $"Please wait {remainingSeconds}s before requesting a new confirmation code.",
                        metadata: new Dictionary<string, string> { ["cooldownSecondsRemaining"] = remainingSeconds.ToString() })
            ];
        }

        return Array.Empty<RuleViolation>();
    }
}

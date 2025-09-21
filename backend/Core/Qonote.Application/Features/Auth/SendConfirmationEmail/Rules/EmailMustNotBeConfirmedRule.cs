using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.SendConfirmationEmail.Rules;

public sealed class EmailMustNotBeConfirmedRule : IBusinessRule<SendConfirmationEmailCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public EmailMustNotBeConfirmedRule(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public int Order => 1;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(SendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        // We assume UserMustExistByEmailRule has already run, so user is not null here.
        if (user is not null && user.EmailConfirmed)
        {
            return new[] { new RuleViolation(nameof(request.Email), "This email address has already been confirmed.") };
        }

        return Array.Empty<RuleViolation>();
    }
}

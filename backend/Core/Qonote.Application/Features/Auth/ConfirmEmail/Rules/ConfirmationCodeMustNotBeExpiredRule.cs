using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.ConfirmEmail.Rules;

public sealed class ConfirmationCodeMustNotBeExpiredRule : IBusinessRule<ConfirmEmailCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmationCodeMustNotBeExpiredRule(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public int Order => 2;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim();
        var user = await _userManager.FindByEmailAsync(email!);
        // We assume UserMustExistRule has already run, so user is not null here.
        if (user is not null && user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
        {
            return [new RuleViolation(nameof(request.Code), "Confirmation code has expired.")];
        }

        return Array.Empty<RuleViolation>();
    }
}

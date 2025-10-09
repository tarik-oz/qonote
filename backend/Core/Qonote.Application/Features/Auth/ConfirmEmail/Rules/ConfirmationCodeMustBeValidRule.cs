using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.ConfirmEmail.Rules;

public sealed class ConfirmationCodeMustBeValidRule : IBusinessRule<ConfirmEmailCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmationCodeMustBeValidRule(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public int Order => 1;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim();
        var user = await _userManager.FindByEmailAsync(email!);
        // We assume UserMustExistRule has already run, so user is not null here.
        if (user is not null && user.EmailConfirmationCode != request.Code)
        {
            return [new RuleViolation(nameof(request.Code), "Confirmation code is not valid.")];
        }

        return Array.Empty<RuleViolation>();
    }
}

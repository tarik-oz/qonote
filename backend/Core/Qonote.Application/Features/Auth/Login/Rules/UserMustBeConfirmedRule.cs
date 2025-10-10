using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Login.Rules;

public sealed class UserMustBeConfirmedRule : IBusinessRule<LoginCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserMustBeConfirmedRule(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public int Order => 2;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email?.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedEmail!);

        // This rule runs after user and password have been validated.
        // We can assume user is not null here.
        if (user is not null && !user.EmailConfirmed)
        {
            return [new RuleViolation("Auth.Login", "Please confirm your email before logging in.")];
        }

        return Array.Empty<RuleViolation>();
    }
}

using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Login.Rules;

public sealed class UserAndPasswordMustBeValidRule : IBusinessRule<LoginCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAndPasswordMustBeValidRule(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public int Order => 1;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email?.Trim();

        var user = await _userManager.FindByEmailAsync(normalizedEmail!);
        if (user is null)
        {
            return [new RuleViolation("Auth.Login", "Invalid email or password.")];
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return [new RuleViolation("Auth.Login", "Invalid email or password.")];
        }

        return Array.Empty<RuleViolation>();
    }
}

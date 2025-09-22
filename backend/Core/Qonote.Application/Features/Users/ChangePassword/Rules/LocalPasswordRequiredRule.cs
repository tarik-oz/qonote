using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Requests;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Users.ChangePassword.Rules;

public sealed class LocalPasswordRequiredRule : IBusinessRule<ChangePasswordCommand>
{
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;

    public LocalPasswordRequiredRule(ICurrentUserService currentUser, UserManager<ApplicationUser> userManager)
    {
        _currentUser = currentUser;
        _userManager = userManager;
    }

    // Run right after user existence, before checking old password
    public int Order => 1;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            return new[] { new RuleViolation("User", "Authenticated user not found.") };

        var user = await _userManager.FindByIdAsync(_currentUser.UserId!);
        if (user is null)
            return new[] { new RuleViolation("User", "Authenticated user not found.") };

        // If user has no local password (external-only), block password change
        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return new[] { new RuleViolation("Password", "Password change is not available for external-login accounts.") };
        }

        return Array.Empty<RuleViolation>();
    }
}

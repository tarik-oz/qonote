using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Users.ChangePassword.Rules;

public sealed class OldPasswordMustBeCorrectRule : IBusinessRule<ChangePasswordCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;

    public OldPasswordMustBeCorrectRule(UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public int Order => 2;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var violations = new List<RuleViolation>();
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            // User existence is validated elsewhere; no violation here
            return violations;
        }

        var user = await _userManager.FindByIdAsync(_currentUser.UserId!);
        if (user == null)
        {
            // Let UserMustExistRule handle this
            return violations;
        }

        var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, request.OldPassword);
        if (!isPasswordCorrect)
        {
            violations.Add(new RuleViolation(nameof(request.OldPassword), "The current password provided is incorrect."));
        }

        return violations;
    }
}

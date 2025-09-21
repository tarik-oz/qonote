using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Register.Rules;

public sealed class EmailMustBeUniqueRule : IBusinessRule<RegisterUserCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public EmailMustBeUniqueRule(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public int Order => 1;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            return new[] { new RuleViolation(nameof(request.Email), "A user with this email address already exists.") };
        }

        return Array.Empty<RuleViolation>();
    }
}

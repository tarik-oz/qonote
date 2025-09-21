using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Register.Rules;

public class EmailMustBeUniqueRule : IBusinessRule<RegisterUserCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public EmailMustBeUniqueRule(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<RuleViolation>> CheckAsync(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return new[] { new RuleViolation("Email", $"The email '{request.Email}' is already registered.") };
        }

        return Array.Empty<RuleViolation>();
    }
}

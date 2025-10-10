using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Requests;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth._Rules;

public sealed class UserMustExistByEmailRule<TRequest> : IBusinessRule<TRequest> where TRequest : IEmailBearingRequest
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserMustExistByEmailRule(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public int Order => 0;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(TRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return [new RuleViolation(typeof(TRequest).Name.Replace("Command", ""), "User not found.")];
        }

        return Array.Empty<RuleViolation>();
    }
}

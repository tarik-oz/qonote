using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Requests;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Users._Rules;

public sealed class UserMustExistRule<TRequest> : IBusinessRule<TRequest> where TRequest : IAuthenticatedRequest
{
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserMustExistRule(ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager)
    {
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public int Order => 0;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(TRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId) || await _userManager.FindByIdAsync(userId) is null)
        {
            return new[] { new RuleViolation("User", "Authenticated user not found.") };
        }
        return Array.Empty<RuleViolation>();
    }
}

using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Features.Auth.Logout;
using Qonote.Core.Application.Features.Users.UpdateProfileInfo;
using Qonote.Core.Application.Features.Users.UpdateProfilePicture;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Users._Rules;

public class UserMustExistRule :
    IBusinessRule<UpdateProfileInfoCommand>,
    IBusinessRule<UpdateProfilePictureCommand>,
    IBusinessRule<LogoutCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserMustExistRule(ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager)
    {
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    private async Task<IEnumerable<RuleViolation>> CheckUserExistenceAsync()
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId) || await _userManager.FindByIdAsync(userId) is null)
        {
            return new[] { new RuleViolation("User", "Authenticated user not found.") };
        }
        return Array.Empty<RuleViolation>();
    }

    // This implementation is shared across all commands this rule applies to.
    public Task<IEnumerable<RuleViolation>> CheckAsync(UpdateProfileInfoCommand request, CancellationToken cancellationToken) => CheckUserExistenceAsync();
    public Task<IEnumerable<RuleViolation>> CheckAsync(UpdateProfilePictureCommand request, CancellationToken cancellationToken) => CheckUserExistenceAsync();
    public Task<IEnumerable<RuleViolation>> CheckAsync(LogoutCommand request, CancellationToken cancellationToken) => CheckUserExistenceAsync();
}
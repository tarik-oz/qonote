using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public LogoutCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // The UserMustExistRule is now handled by the BusinessRulesBehavior.
        var user = await _userManager.FindByIdAsync(_currentUserService.UserId!);

        user!.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        return Unit.Value;
    }
}

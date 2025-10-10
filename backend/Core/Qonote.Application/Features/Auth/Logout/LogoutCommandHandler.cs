using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService, ILogger<LogoutCommandHandler> logger)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // The UserMustExistRule is now handled by the BusinessRulesBehavior.
        var user = await _userManager.FindByIdAsync(_currentUserService.UserId!);

        user!.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User logged out. {UserId}", user.Id);

        return Unit.Value;
    }
}

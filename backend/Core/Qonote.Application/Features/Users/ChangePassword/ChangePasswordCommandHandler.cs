using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Users.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUser, ILogger<ChangePasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // User existence and old password correctness are validated by business rules
        var user = await _userManager.FindByIdAsync(_currentUser.UserId!);

        var result = await _userManager.ChangePasswordAsync(user!, request.OldPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));
        }

        _logger.LogInformation("Password changed successfully. UserId={UserId}", user!.Id);

        return Unit.Value;
    }
}

using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public LogoutCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        return Unit.Value;
    }
}

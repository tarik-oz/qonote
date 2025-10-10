using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Security;

namespace Qonote.Core.Application.Features.Users.DeleteMyAccount;

public sealed class DeleteMyAccountCommandHandler : IRequestHandler<DeleteMyAccountCommand, Unit>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IAccountDeletionService _accountDeletion;
    private readonly ILogger<DeleteMyAccountCommandHandler> _logger;

    public DeleteMyAccountCommandHandler(ICurrentUserService currentUser, IAccountDeletionService accountDeletion, ILogger<DeleteMyAccountCommandHandler> logger)
    {
        _currentUser = currentUser;
        _accountDeletion = accountDeletion;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteMyAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        await _accountDeletion.DeleteUserAsync(userId, cancellationToken);
        _logger.LogInformation("Account deletion requested. UserId={UserId}", userId);
        return Unit.Value;
    }
}



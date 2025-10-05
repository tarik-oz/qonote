using MediatR;
using Qonote.Core.Application.Abstractions.Security;

namespace Qonote.Core.Application.Features.Users.DeleteMyAccount;

public sealed class DeleteMyAccountCommandHandler : IRequestHandler<DeleteMyAccountCommand, Unit>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IAccountDeletionService _accountDeletion;

    public DeleteMyAccountCommandHandler(ICurrentUserService currentUser, IAccountDeletionService accountDeletion)
    {
        _currentUser = currentUser;
        _accountDeletion = accountDeletion;
    }

    public async Task<Unit> Handle(DeleteMyAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        await _accountDeletion.DeleteUserAsync(userId, cancellationToken);
        return Unit.Value;
    }
}



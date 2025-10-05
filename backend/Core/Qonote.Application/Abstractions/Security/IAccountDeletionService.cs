namespace Qonote.Core.Application.Abstractions.Security;

public interface IAccountDeletionService
{
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken);
}



namespace Qonote.Core.Application.Abstractions.Subscriptions;

public interface ILimitCheckerService
{
    Task EnsureUserCanCreateNoteAsync(string userId, CancellationToken cancellationToken = default);
}

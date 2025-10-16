namespace Qonote.Core.Application.Abstractions.Subscriptions;

public interface ILimitCheckerService
{
    // Checks remaining quota and, if allowed, increments usage for the current period in the same transaction scope.
    Task EnsureAndConsumeNoteQuotaAsync(string userId, CancellationToken cancellationToken = default);
}

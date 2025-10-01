namespace Qonote.Core.Application.Abstractions.Caching;

public interface ICacheInvalidationService
{
    Task RemoveSidebarForAsync(IEnumerable<string> userIds, CancellationToken ct);
    Task RemoveMeAsync(string userId, CancellationToken ct);
}

using Qonote.Core.Application.Abstractions.Caching;

namespace Qonote.Infrastructure.Infrastructure.Caching;

public sealed class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheService _cache;

    public CacheInvalidationService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task RemoveSidebarForAsync(IEnumerable<string> userIds, CancellationToken ct)
    {
        // Deduplicate just in case caller didn't
        foreach (var uid in userIds.Distinct())
        {
            var key = $"qonote:sidebar:{uid}";
            await _cache.RemoveAsync(key, ct);
        }
    }

    public async Task RemoveMeAsync(string userId, CancellationToken ct)
    {
        var key = $"qonote:me:{userId}";
        await _cache.RemoveAsync(key, ct);
    }
}

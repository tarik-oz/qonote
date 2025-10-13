using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Caching;

namespace Qonote.Infrastructure.Infrastructure.Caching;

public sealed class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheService _cache;
    private readonly ILogger<CacheInvalidationService> _logger;

    public CacheInvalidationService(ICacheService cache, ILogger<CacheInvalidationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task RemoveSidebarForAsync(IEnumerable<string> userIds, CancellationToken ct)
    {
        // Deduplicate just in case caller didn't
        foreach (var uid in userIds.Distinct())
        {
            var key = $"qonote:sidebar:{uid}";
            await _cache.RemoveAsync(key, ct);
            _logger.LogDebug("Cache invalidated (sidebar). userId={UserId}", uid);
        }
    }

    public async Task RemoveMeAsync(string userId, CancellationToken ct)
    {
        var key = $"qonote:me:{userId}";
        await _cache.RemoveAsync(key, ct);
        _logger.LogDebug("Cache invalidated (me). userId={UserId}", userId);
    }
}

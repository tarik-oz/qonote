using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Qonote.Core.Application.Abstractions.Caching;
using StackExchange.Redis;

namespace Qonote.Infrastructure.Infrastructure.Caching;

public sealed class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly CacheOptions _options;
    private readonly IConnectionMultiplexer _redis;

    public CacheService(IDistributedCache cache, IOptions<CacheOptions> options, IConnectionMultiplexer redis)
    {
        _cache = cache;
        _options = options.Value;
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        if (!_options.Enabled) return default;
        var bytes = await _cache.GetAsync(key, ct);
        if (bytes is null || bytes.Length == 0) return default;
        return JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan ttl, CancellationToken ct)
    {
        if (!_options.Enabled)
        {
            return await factory(ct);
        }

        var existing = await GetAsync<T>(key, ct);
        if (existing is not null) return existing;

        var created = await factory(ct);
        await SetAsync(key, created!, ttl, ct);
        return created;
    }

    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        if (!_options.Enabled) return;
        await _cache.RemoveAsync(key, ct);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
    {
        if (!_options.Enabled) return;
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, ct);
    }

    public async Task<long> IncrementAsync(string key, CancellationToken ct)
    {
        // Atomic INCR with a TTL safeguard on the version key
        var db = _redis.GetDatabase();
        var nextVal = await db.StringIncrementAsync(key);
        var ttl = await db.KeyTimeToLiveAsync(key);
        if (ttl is null || ttl <= TimeSpan.Zero)
        {
            await db.KeyExpireAsync(key, TimeSpan.FromHours(6));
        }
        return (long)nextVal;
    }
}

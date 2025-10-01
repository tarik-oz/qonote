using Qonote.Core.Application.Abstractions.Caching;

namespace Qonote.Infrastructure.Infrastructure.Caching;

public sealed class NoOpCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken ct) => Task.FromResult<T?>(default);

    public async Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan ttl, CancellationToken ct)
    {
        return await factory(ct);
    }

    public Task RemoveAsync(string key, CancellationToken ct) => Task.CompletedTask;

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct) => Task.CompletedTask;

    public Task<long> IncrementAsync(string key, CancellationToken ct) => Task.FromResult(0L);
}

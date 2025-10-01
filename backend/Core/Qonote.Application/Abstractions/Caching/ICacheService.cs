namespace Qonote.Core.Application.Abstractions.Caching;

public interface ICacheService
{
    Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan ttl, CancellationToken ct);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct);
    Task<T?> GetAsync<T>(string key, CancellationToken ct);
    Task RemoveAsync(string key, CancellationToken ct);
    Task<long> IncrementAsync(string key, CancellationToken ct);
}

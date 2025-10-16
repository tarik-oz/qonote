using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Qonote.Presentation.Api.Infrastructure.Health;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the standard 'PING' as a cheap liveness check to Redis
            var db = _redis.GetDatabase();
            var pong = await db.PingAsync();
            return HealthCheckResult.Healthy($"Redis responded in {pong.TotalMilliseconds:F0} ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis ping failed", ex);
        }
    }
}

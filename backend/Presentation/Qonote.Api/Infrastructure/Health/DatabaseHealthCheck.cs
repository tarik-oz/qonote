using Microsoft.Extensions.Diagnostics.HealthChecks;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Presentation.Api.Infrastructure.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _db;
    public DatabaseHealthCheck(ApplicationDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _db.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database connection OK")
                : HealthCheckResult.Unhealthy("Database cannot connect");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database check failed", ex);
        }
    }
}

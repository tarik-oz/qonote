using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Qonote.Presentation.Api.Infrastructure.Health;

public sealed class BlobStorageHealthCheck : IHealthCheck
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageHealthCheck(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Cheap call: list containers with max 1 result to validate connectivity and auth
            await foreach (var _ in _blobServiceClient.GetBlobContainersAsync().WithCancellation(cancellationToken))
            {
                break; // just ensure enumeration works
            }
            return HealthCheckResult.Healthy("Blob Storage reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Blob Storage unreachable", ex);
        }
    }
}

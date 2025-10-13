using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Qonote.Infrastructure.Infrastructure.Storage;

/// <summary>
/// Ensures blob containers have the desired public access policy on app startup.
/// This prevents stale public containers from defeating SAS expirations.
/// </summary>
public sealed class BlobContainerPolicyEnforcer : IHostedService
{
    private readonly BlobServiceClient _serviceClient;
    private readonly ILogger<BlobContainerPolicyEnforcer> _logger;

    public BlobContainerPolicyEnforcer(IOptions<BlobStorageSettings> options, ILogger<BlobContainerPolicyEnforcer> logger)
    {
        _serviceClient = new BlobServiceClient(options.Value.ConnectionString);
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await EnsurePolicyAsync("profile-pictures", PublicAccessType.None, cancellationToken);
        await EnsurePolicyAsync("thumbnails", PublicAccessType.Blob, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsurePolicyAsync(string containerName, PublicAccessType desired, CancellationToken ct)
    {
        try
        {
            var container = _serviceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(desired, cancellationToken: ct);
            await container.SetAccessPolicyAsync(desired, cancellationToken: ct);
            _logger.LogInformation("Container access enforced. container={Container}, access={Access}", containerName, desired);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug("Container not found (skip policy). container={Container}", containerName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enforce container access policy. container={Container}", containerName);
        }
    }
}

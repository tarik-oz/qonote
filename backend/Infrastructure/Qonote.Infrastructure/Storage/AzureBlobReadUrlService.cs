using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qonote.Core.Application.Abstractions.Storage;

namespace Qonote.Infrastructure.Infrastructure.Storage;

public sealed class AzureBlobReadUrlService : IFileReadUrlService
{
    private readonly BlobServiceClient _serviceClient;
    private readonly ILogger<AzureBlobReadUrlService> _logger;

    public AzureBlobReadUrlService(IOptions<BlobStorageSettings> options, ILogger<AzureBlobReadUrlService> logger)
    {
        _serviceClient = new BlobServiceClient(options.Value.ConnectionString);
        _logger = logger;
    }

    public Task<string> GetReadUrlAsync(string originalUrl, TimeSpan ttl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
        {
            return Task.FromResult(originalUrl);
        }

        // Parse container and blob name from URL
        var uri = new Uri(originalUrl);
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            // Not a blob-looking URL; return as-is
            return Task.FromResult(originalUrl);
        }

        // Handle both Azure (account in host) and Azurite (account in first path segment)
        // Example Azure: https://account.blob.core.windows.net/container/blob
        //   segments: [container, ...blob]
        // Example Azurite: http://127.0.0.1:10000/devstoreaccount1/container/blob
        //   segments: [devstoreaccount1, container, ...blob]
        var accountName = _serviceClient.AccountName; // e.g., 'devstoreaccount1' on Azurite
        int containerIndex = 0;
        if (segments[0].Equals(accountName, StringComparison.OrdinalIgnoreCase))
        {
            // Skip account segment
            if (segments.Length < 3)
            {
                return Task.FromResult(originalUrl);
            }
            containerIndex = 1;
        }

        var containerName = segments[containerIndex];
        var blobName = string.Join('/', segments.Skip(containerIndex + 1));

        var container = _serviceClient.GetBlobContainerClient(containerName);
        var blob = container.GetBlobClient(blobName);

        if (!blob.CanGenerateSasUri)
        {
            if (ttl > TimeSpan.Zero)
            {
                _logger.LogWarning("Cannot generate SAS URL; returning original. This will not enforce TTL. container={Container}", containerName);
            }
            // Fallback: return original URL (works if container is public)
            return Task.FromResult(originalUrl);
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresOn = DateTimeOffset.UtcNow.Add(ttl)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blob.GenerateSasUri(sasBuilder);
        return Task.FromResult(sasUri.ToString());
    }
}

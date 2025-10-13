using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qonote.Core.Application.Abstractions.Storage;

namespace Qonote.Infrastructure.Infrastructure.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IOptions<BlobStorageSettings> blobStorageSettings, ILogger<AzureBlobStorageService> logger)
    {
        _blobServiceClient = new BlobServiceClient(blobStorageSettings.Value.ConnectionString);
        _logger = logger;
    }

    public async Task<string> UploadAsync(IFormFile file, string containerName, string fileName)
    {
        await using var stream = file.OpenReadStream();
        _logger.LogInformation("Blob upload (form file) started. container={Container}, fileName={FileName}, contentType={ContentType}, length={Length}", containerName, fileName, file.ContentType, file.Length);
        var url = await UploadAsync(stream, containerName, fileName, file.ContentType);
        _logger.LogInformation("Blob upload (form file) done. container={Container}, fileName={FileName}", containerName, fileName);
        return url;
    }

    public async Task<string> UploadAsync(Stream stream, string containerName, string fileName, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var accessType = GetAccessTypeForContainer(containerName);
        if (accessType == PublicAccessType.None)
        {
            _logger.LogDebug("Ensuring private container. container={Container}", containerName);
        }
        await containerClient.CreateIfNotExistsAsync(accessType);
        try
        {
            // If container already existed with a different access level, enforce the desired policy.
            await containerClient.SetAccessPolicyAsync(accessType);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "SetAccessPolicyAsync failed (may be due to identical policy or permissions). container={Container}", containerName);
        }

        var blobClient = containerClient.GetBlobClient(fileName);
        _logger.LogDebug("Blob upload started. container={Container}, fileName={FileName}, contentType={ContentType}", containerName, fileName, contentType);
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
        _logger.LogDebug("Blob upload completed. container={Container}, fileName={FileName}", containerName, fileName);
        return blobClient.Uri.AbsoluteUri;
    }

    public async Task DeleteAsync(string fileUrl, string containerName)
    {
        // Prefer parsing from URL to support both Azure and Azurite formats
        var uri = new Uri(fileUrl);
        var builder = new BlobUriBuilder(uri);
        var parsedContainer = string.IsNullOrWhiteSpace(builder.BlobContainerName) ? containerName : builder.BlobContainerName;
        var parsedBlobName = builder.BlobName;
        var containerClient = _blobServiceClient.GetBlobContainerClient(parsedContainer);
        var blobClient = containerClient.GetBlobClient(parsedBlobName);
        _logger.LogInformation("Blob delete attempted. container={Container}, blobName={BlobName}", parsedContainer, parsedBlobName);
        await blobClient.DeleteIfExistsAsync();
        _logger.LogInformation("Blob delete completed. container={Container}, blobName={BlobName}", parsedContainer, parsedBlobName);
    }

    private static PublicAccessType GetAccessTypeForContainer(string containerName)
    {
        // Keep profile pictures private; others (e.g., thumbnails) can stay public
        return containerName.Equals("profile-pictures", StringComparison.OrdinalIgnoreCase)
            ? PublicAccessType.None
            : PublicAccessType.Blob;
    }
}

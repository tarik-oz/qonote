using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Qonote.Core.Application.Abstractions.Storage;

namespace Qonote.Infrastructure.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IOptions<BlobStorageSettings> blobStorageSettings)
    {
        _blobServiceClient = new BlobServiceClient(blobStorageSettings.Value.ConnectionString);
    }

    public async Task<string> UploadAsync(IFormFile file, string containerName, string fileName)
    {
        await using var stream = file.OpenReadStream();
        return await UploadAsync(stream, containerName, fileName, file.ContentType);
    }

    public async Task<string> UploadAsync(Stream stream, string containerName, string fileName, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });

        return blobClient.Uri.AbsoluteUri;
    }

    public async Task DeleteAsync(string fileUrl, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var uri = new Uri(fileUrl);
        // Blob name may contain folders; remove leading '/{container}/'
        var absolutePath = uri.AbsolutePath; // e.g., /thumbnails/userId/notes/123/thumbnail.jpg
        var prefix = "/" + containerName + "/";
        var blobName = absolutePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? absolutePath.Substring(prefix.Length)
            : absolutePath.TrimStart('/');
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}

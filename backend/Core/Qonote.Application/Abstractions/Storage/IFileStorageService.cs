using Microsoft.AspNetCore.Http;

namespace Qonote.Core.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string containerName, string fileName);
    Task<string> UploadAsync(Stream stream, string containerName, string fileName, string contentType);
    Task DeleteAsync(string fileUrl, string containerName);
}

using Microsoft.AspNetCore.Http;
using Qonote.Core.Application.Abstractions.Media;
using Qonote.Core.Application.Abstractions.Storage;

namespace Qonote.Infrastructure.Media;

public sealed class ImageService : IImageService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly HttpClient _httpClient;

    private const string ThumbnailsContainer = "thumbnails";
    private const string ProfilePicturesContainer = "profile-pictures";

    public ImageService(IFileStorageService fileStorageService, HttpClient httpClient)
    {
        _fileStorageService = fileStorageService;
        _httpClient = httpClient;
    }

    public async Task<string?> UploadNoteThumbnailFromUrlAsync(string imageUrl, string userId, int noteId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(imageUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            var ext = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
            var fileName = $"{userId}/notes/{noteId}/thumbnail{ext}"; // hierarchical path
            var blobUrl = await _fileStorageService.UploadAsync(stream, ThumbnailsContainer, fileName, contentType);
            return blobUrl;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile file, string userId, CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext))
        {
            ext = file.ContentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }
        var fileName = $"{userId}{ext}"; // flat naming for now, can switch to {userId}/profile/ later
        return await _fileStorageService.UploadAsync(file, ProfilePicturesContainer, fileName);
    }

    public async Task<string?> UploadProfilePictureFromUrlAsync(string imageUrl, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(imageUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            var ext = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
            var fileName = $"{userId}{ext}";
            var blobUrl = await _fileStorageService.UploadAsync(stream, ProfilePicturesContainer, fileName, contentType);
            return blobUrl;
        }
        catch
        {
            return null;
        }
    }
}

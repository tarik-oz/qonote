using Microsoft.AspNetCore.Http;
using Qonote.Core.Application.Abstractions.Media;
using Qonote.Core.Application.Abstractions.Storage;
using SkiaSharp;

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

            // Convert to WebP
            // Target max width ~ 800px, height auto; quality ~ 80
            var webp = ConvertToWebp(stream, maxWidth: 800, maxHeight: 800, quality: 80);
            if (webp is null)
            {
                return null;
            }

            await using var webpStream = webp.Value.stream;
            webpStream.Position = 0;
            var fileName = $"{userId}/notes/{noteId}/thumbnail{webp.Value.ext}"; // hierarchical path
            var blobUrl = await _fileStorageService.UploadAsync(webpStream, ThumbnailsContainer, fileName, webp.Value.contentType);
            return blobUrl;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile file, string userId, CancellationToken cancellationToken)
    {
        await using var input = file.OpenReadStream();
        // Convert to WebP, square-ish max size ~ 256px for header/profile usage
        var webp = ConvertToWebp(input, maxWidth: 256, maxHeight: 256, quality: 85);
        if (webp is null)
        {
            // Fallback: upload original
            var extFallback = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extFallback))
            {
                extFallback = file.ContentType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    _ => ".jpg"
                };
            }
            var fileNameFallback = $"{userId}{extFallback}";
            return await _fileStorageService.UploadAsync(file, ProfilePicturesContainer, fileNameFallback);
        }

        await using var webpStream = webp.Value.stream;
        webpStream.Position = 0;
        var fileName = $"{userId}{webp.Value.ext}"; // flat naming for now, can switch to {userId}/profile/ later
        return await _fileStorageService.UploadAsync(webpStream, ProfilePicturesContainer, fileName, webp.Value.contentType);
    }

    public async Task<string?> UploadProfilePictureFromUrlAsync(string imageUrl, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(imageUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            // Convert to WebP, target ~256px, quality 85
            var webp = ConvertToWebp(stream, maxWidth: 256, maxHeight: 256, quality: 85);
            if (webp is null)
            {
                return null;
            }

            await using var webpStream = webp.Value.stream;
            webpStream.Position = 0;
            var fileName = $"{userId}{webp.Value.ext}";
            var blobUrl = await _fileStorageService.UploadAsync(webpStream, ProfilePicturesContainer, fileName, webp.Value.contentType);
            return blobUrl;
        }
        catch
        {
            return null;
        }
    }

    private (MemoryStream stream, string ext, string contentType)? ConvertToWebp(Stream input, int maxWidth, int maxHeight, int quality)
    {
        try
        {
            // Decode
            using var managed = new MemoryStream();
            input.CopyTo(managed);
            managed.Position = 0;
            using var original = SKBitmap.Decode(managed);
            if (original == null)
            {
                return null;
            }

            // Compute target size preserving aspect ratio, no upscaling
            int targetWidth = original.Width;
            int targetHeight = original.Height;
            if (targetWidth > maxWidth || targetHeight > maxHeight)
            {
                var widthRatio = (double)maxWidth / targetWidth;
                var heightRatio = (double)maxHeight / targetHeight;
                var scale = Math.Min(widthRatio, heightRatio);
                targetWidth = (int)Math.Floor(targetWidth * scale);
                targetHeight = (int)Math.Floor(targetHeight * scale);
            }

            SKBitmap toEncode = original;
            if (targetWidth != original.Width || targetHeight != original.Height)
            {
                var resized = new SKBitmap(targetWidth, targetHeight, original.ColorType, original.AlphaType);
                original.ScalePixels(resized, SKFilterQuality.High);
                toEncode = resized;
            }

            using var image = SKImage.FromBitmap(toEncode);
            using var data = image.Encode(SKEncodedImageFormat.Webp, Math.Clamp(quality, 1, 100));
            var output = new MemoryStream();
            data.SaveTo(output);
            output.Position = 0;
            return (output, ".webp", "image/webp");
        }
        catch
        {
            return null;
        }
    }
}

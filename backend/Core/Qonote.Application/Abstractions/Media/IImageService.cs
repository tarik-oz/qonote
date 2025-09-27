using Microsoft.AspNetCore.Http;

namespace Qonote.Core.Application.Abstractions.Media;

/// <summary>
/// Central image helper for uploads and future processing (webp, crop, resize).
/// Current implementation is pass-through; exposed to avoid duplicating logic in handlers.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Downloads a remote thumbnail and uploads it to blob storage using a stable hierarchical name.
    /// Returns the blob URL or null when the remote fetch fails.
    /// </summary>
    Task<string?> UploadNoteThumbnailFromUrlAsync(string imageUrl, string userId, int noteId, CancellationToken cancellationToken);

    /// <summary>
    /// Uploads a user-provided profile picture and returns the blob URL.
    /// </summary>
    Task<string> UploadProfilePictureAsync(IFormFile file, string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Downloads a remote profile picture (e.g., from Google) and uploads it to blob storage.
    /// Returns the blob URL or null if the download fails.
    /// </summary>
    Task<string?> UploadProfilePictureFromUrlAsync(string imageUrl, string userId, CancellationToken cancellationToken);
}

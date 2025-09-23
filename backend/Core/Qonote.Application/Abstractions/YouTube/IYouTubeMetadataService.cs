using Qonote.Core.Application.Abstractions.YouTube.Models;

namespace Qonote.Core.Application.Abstractions.YouTube;

public interface IYouTubeMetadataService
{
    // Given a YouTube video ID, returns basic metadata required by the app.
    Task<YouTubeVideoMetadata> GetVideoMetadataAsync(string videoId, CancellationToken cancellationToken = default);
}

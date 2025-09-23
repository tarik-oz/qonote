using Qonote.Core.Application.Abstractions.YouTube.Models;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Abstractions.Factories;

public interface INoteFactory
{
    Note CreateFromYouTubeMetadata(YouTubeVideoMetadata meta, string userId, string youtubeUrl, string? customTitle);
}

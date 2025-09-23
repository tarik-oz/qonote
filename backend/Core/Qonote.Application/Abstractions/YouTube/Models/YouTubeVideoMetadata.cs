namespace Qonote.Core.Application.Abstractions.YouTube.Models;

public sealed class YouTubeVideoMetadata
{
    public required string VideoId { get; init; }
    public required string Title { get; init; }
    public required string ChannelTitle { get; init; }
    public required string ThumbnailUrl { get; init; }
    public required TimeSpan Duration { get; init; }
    public string? Description { get; init; }
}

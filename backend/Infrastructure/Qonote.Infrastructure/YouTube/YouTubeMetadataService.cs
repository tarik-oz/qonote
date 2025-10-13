using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qonote.Core.Application.Abstractions.YouTube;
using Qonote.Core.Application.Abstractions.YouTube.Models;
using Qonote.Core.Application.Exceptions;

namespace Qonote.Infrastructure.Infrastructure.YouTube;

public sealed class YouTubeMetadataService : IYouTubeMetadataService
{
    private readonly HttpClient _httpClient;
    private readonly YouTubeSettings _settings;
    private readonly ILogger<YouTubeMetadataService> _logger;

    public YouTubeMetadataService(HttpClient httpClient, IOptions<YouTubeSettings> settings, ILogger<YouTubeMetadataService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<YouTubeVideoMetadata> GetVideoMetadataAsync(string videoId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("YouTubeSettings.ApiKey is not configured.");
        }

        // parts: snippet (title, description, thumbnails, channelTitle), contentDetails (duration)
        var url = $"https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails&id={Uri.EscapeDataString(videoId)}&key={_settings.ApiKey}";
        _logger.LogDebug("YouTube fetch metadata started. videoId={VideoId}", videoId);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("YouTube API responded with {StatusCode}: {Body}", (int)response.StatusCode, body);
            throw new ExternalServiceException(
                provider: "YouTube",
                message: "Failed to fetch YouTube metadata.",
                statusCode: (int)response.StatusCode,
                errors: new Dictionary<string, string[]>
                {
                    { "response", new [] { body } }
                });
        }

        var json = await response.Content.ReadFromJsonAsync<YouTubeVideosListResponse>(cancellationToken: cancellationToken);
        var item = json?.Items?.FirstOrDefault();
        if (item is null)
        {
            _logger.LogInformation("YouTube video not found. videoId={VideoId}", videoId);
            throw new NotFoundException("YouTube video not found or inaccessible.");
        }

        var duration = System.Xml.XmlConvert.ToTimeSpan(item.ContentDetails.Duration);
        var thumbnailUrl = item.Snippet.Thumbnails?.MaxRes?.Url
                           ?? item.Snippet.Thumbnails?.High?.Url
                           ?? item.Snippet.Thumbnails?.Medium?.Url
                           ?? item.Snippet.Thumbnails?.Standard?.Url
                           ?? item.Snippet.Thumbnails?.Default?.Url
                           ?? string.Empty;

        var result = new YouTubeVideoMetadata
        {
            VideoId = videoId,
            Title = item.Snippet.Title ?? string.Empty,
            ChannelTitle = item.Snippet.ChannelTitle ?? string.Empty,
            ThumbnailUrl = thumbnailUrl,
            Duration = duration,
            Description = item.Snippet.Description
        };
        _logger.LogInformation("YouTube metadata fetched. videoId={VideoId}, duration={Duration}s, hasThumbnail={HasThumb}", videoId, (int)duration.TotalSeconds, !string.IsNullOrWhiteSpace(thumbnailUrl));
        return result;
    }

    // minimal types for deserialization of YouTube API
    private sealed class YouTubeVideosListResponse
    {
        public List<Item> Items { get; init; } = new();

        public sealed class Item
        {
            public SnippetPart Snippet { get; init; } = new();
            public ContentDetailsPart ContentDetails { get; init; } = new();
        }

        public sealed class SnippetPart
        {
            public string? Title { get; init; }
            public string? Description { get; init; }
            public string? ChannelTitle { get; init; }
            public ThumbnailsPart? Thumbnails { get; init; }
        }

        public sealed class ContentDetailsPart
        {
            public string Duration { get; init; } = "PT0S"; // ISO 8601 duration
        }

        public sealed class ThumbnailsPart
        {
            public ThumbnailPart? Default { get; init; }
            public ThumbnailPart? Medium { get; init; }
            public ThumbnailPart? High { get; init; }
            public ThumbnailPart? Standard { get; init; }
            public ThumbnailPart? MaxRes { get; init; }
        }

        public sealed class ThumbnailPart
        {
            public string? Url { get; init; }
        }
    }
}

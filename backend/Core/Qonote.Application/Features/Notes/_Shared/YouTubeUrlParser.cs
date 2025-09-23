using System.Text.RegularExpressions;

namespace Qonote.Core.Application.Features.Notes._Shared;

public static class YouTubeUrlParser
{
    // Supports youtu.be/<id>, youtube.com/watch?v=<id>, youtube.com/shorts/<id>, with or without params
    private static readonly Regex VideoIdRegex = new(
        pattern: @"(?:youtu\.be/|v=|/shorts/)([a-zA-Z0-9_-]{6,})",
        options: RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string? TryExtractVideoId(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        var match = VideoIdRegex.Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }
}

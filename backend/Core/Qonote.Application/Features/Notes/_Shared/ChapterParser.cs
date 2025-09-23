using System.Text.RegularExpressions;

namespace Qonote.Core.Application.Features.Notes._Shared;

public static class ChapterParser
{
    private static readonly Regex TimestampRegex = new(
        pattern: @"(?<!\d)(?:(\d{1,2}):)?(\d{1,2}):(\d{2})(?!\d)",
        options: RegexOptions.Compiled);

    public sealed record Chapter(string Title, TimeSpan Start);

    public static List<(string Title, TimeSpan Start, TimeSpan End)> Parse(string? description, TimeSpan totalDuration)
    {
        var chapters = new List<Chapter>();
        if (string.IsNullOrWhiteSpace(description))
        {
            return new();
        }

        foreach (var line in description.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var m = TimestampRegex.Match(line);
            if (!m.Success) continue;

            var hasHours = !string.IsNullOrEmpty(m.Groups[1].Value);
            int hours = hasHours ? int.Parse(m.Groups[1].Value) : 0;
            int minutes = int.Parse(m.Groups[2].Value);
            int seconds = int.Parse(m.Groups[3].Value);
            var start = new TimeSpan(hours, minutes, seconds);

            // Title is the remaining text after the matched timestamp
            var title = line[(m.Index + m.Length)..].Trim(" -:\u2013\u2014\t".ToCharArray());
            if (string.IsNullOrWhiteSpace(title)) title = $"Chapter {chapters.Count + 1}";

            // Avoid duplicate/unsorted starts
            if (chapters.Count == 0 || start > chapters[^1].Start)
            {
                chapters.Add(new Chapter(title, start));
            }
        }

        // Enforce basic YouTube rules heuristically:
        // - First chapter near 00:00 (allow small tolerance up to 2s)
        // - At least 3 chapters
        // - Minimum chapter length 10s
        if (chapters.Count < 3) return new();
        if (chapters[0].Start > TimeSpan.FromSeconds(2)) return new();

        // Build ranges and enforce min length
        var ranges = new List<(string Title, TimeSpan Start, TimeSpan End)>();
        for (int i = 0; i < chapters.Count; i++)
        {
            var start = chapters[i].Start;
            var end = i + 1 < chapters.Count ? chapters[i + 1].Start : totalDuration;
            if (end <= start) continue;
            if ((end - start) < TimeSpan.FromSeconds(10)) continue;
            ranges.Add((chapters[i].Title, start, end));
        }

        return ranges.Count >= 3 ? ranges : new();
    }
}

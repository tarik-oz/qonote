using AutoMapper;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Abstractions.YouTube.Models;
using Qonote.Core.Application.Features.Notes._Shared;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Factories;

public sealed class NoteFactory : INoteFactory
{
    private readonly IMapper _mapper;

    public NoteFactory(IMapper mapper)
    {
        _mapper = mapper;
    }

    public Note CreateFromYouTubeMetadata(YouTubeVideoMetadata meta, string userId, string youtubeUrl, string? customTitle)
    {
        var note = _mapper.Map<Note>(meta);
        note.UserId = userId;
        note.YoutubeUrl = youtubeUrl;
        var finalTitle = string.IsNullOrWhiteSpace(customTitle) ? meta.Title : customTitle;
        note.CustomTitle = finalTitle?.Trim() ?? string.Empty;
        note.Sections = new List<Section>();

        // Section 1: Video Info
        note.Sections.Add(new Section
        {
            Title = "Video Info",
            Type = SectionType.VideoInfo,
            StartTime = TimeSpan.Zero,
            EndTime = meta.Duration,
            Order = 0
        });

        // Section 2: General (with empty paragraph block)
        var general = new Section
        {
            Title = "General",
            Type = SectionType.GeneralNote,
            StartTime = TimeSpan.Zero,
            EndTime = meta.Duration,
            Order = 1
        };
        general.Blocks.Add(new Block
        {
            Type = BlockType.Paragraph,
            Order = 0,
            Content = string.Empty
        });
        note.Sections.Add(general);

        // Timestamped sections from chapters (fallback to equal split if none)
        var chapters = ChapterParser.Parse(meta.Description, meta.Duration);
        int order = 2;
        if (chapters.Count > 0)
        {
            foreach (var (title, start, end) in chapters)
            {
                var chapterSection = new Section
                {
                    Title = title,
                    Type = SectionType.Timestamped,
                    StartTime = start,
                    EndTime = end,
                    Order = order++
                };
                chapterSection.Blocks.Add(new Block
                {
                    Type = BlockType.Paragraph,
                    Order = 0,
                    Content = string.Empty
                });
                note.Sections.Add(chapterSection);
            }
        }
        else
        {
            // Create 3 equal timestamped sections over the full duration
            var total = meta.Duration;
            var third = new TimeSpan(total.Ticks / 3);
            var s0 = TimeSpan.Zero;
            var s1 = third;
            var s2 = third + third;
            var s3 = total;

            var parts = new List<(string title, TimeSpan start, TimeSpan end)>
            {
                ("Part 1", s0, s1),
                ("Part 2", s1, s2),
                ("Part 3", s2, s3)
            };
            foreach (var (title, start, end) in parts)
            {
                var section = new Section
                {
                    Title = title,
                    Type = SectionType.Timestamped,
                    StartTime = start,
                    EndTime = end,
                    Order = order++
                };
                section.Blocks.Add(new Block
                {
                    Type = BlockType.Paragraph,
                    Order = 0,
                    Content = string.Empty
                });
                note.Sections.Add(section);
            }
        }

        return note;
    }
}

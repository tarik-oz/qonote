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

        // Timestamped sections from chapters
        var chapters = ChapterParser.Parse(meta.Description, meta.Duration);
        if (chapters.Count > 0)
        {
            int order = 2;
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

        return note;
    }
}

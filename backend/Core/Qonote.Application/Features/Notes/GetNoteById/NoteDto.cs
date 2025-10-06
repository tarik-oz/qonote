using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Notes.GetNoteById;

public sealed class NoteDto
{
    public int Id { get; init; }
    public string CustomTitle { get; init; } = string.Empty;
    public string YoutubeUrl { get; init; } = string.Empty;
    public string VideoTitle { get; init; } = string.Empty;
    public string ThumbnailUrl { get; init; } = string.Empty;
    public string ChannelName { get; init; } = string.Empty;
    public TimeSpan VideoDuration { get; init; }
    public bool IsPublic { get; init; }
    public Guid? PublicShareGuid { get; init; }
    public int Order { get; init; }
    public List<SectionDto> Sections { get; init; } = new();
}

public sealed class SectionDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public int Order { get; init; }
    public SectionType Type { get; init; }
    public bool IsCollapsed { get; set; }
    public List<BlockDto> Blocks { get; init; } = new();
}

public sealed class BlockDto
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public BlockType Type { get; init; }
    public int Order { get; init; }
}

namespace Qonote.Core.Application.Features.Sidebar.GetSidebar;

public sealed class SidebarDto
{
    public string Mode { get; set; } = "flat"; // "grouped" | "flat"
    public List<GroupItemDto>? Groups { get; set; }
    public List<NoteListItemDto>? Notes { get; set; }
}

public sealed class GroupItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public int NoteCount { get; set; }
    public List<string> PreviewThumbnails { get; set; } = new();
}

public sealed class NoteListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int Order { get; set; }
}

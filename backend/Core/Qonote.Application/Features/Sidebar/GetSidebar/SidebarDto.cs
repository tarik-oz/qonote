namespace Qonote.Core.Application.Features.Sidebar.GetSidebar;

public sealed record SidebarDto(
    string Mode,
    List<GroupItemDto>? Groups,
    List<NoteListItemDto>? Notes
);

public sealed record GroupItemDto(
    int Id,
    string Title,
    int Order,
    int NoteCount,
    List<string> PreviewThumbnails
);

public sealed record NoteListItemDto(
    int Id,
    string Title,
    string ThumbnailUrl,
    int Order
);

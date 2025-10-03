using Qonote.Core.Application.Features.Sidebar.GetSidebar;
using Qonote.Core.Application.Features.Notes.SearchNotes;

namespace Qonote.Core.Application.Abstractions.Queries;

public interface INoteQueries
{
    Task<List<NoteListItemDto>> GetOrderedNotesForGroupAsync(int groupId, string userId, int? offset, int? limit, CancellationToken cancellationToken);
    Task<List<NoteListItemDto>> GetOrderedUngroupedNotesAsync(string userId, int? offset, int? limit, CancellationToken cancellationToken);
    Task<int> CountActiveNotesAsync(string userId, CancellationToken cancellationToken);
    Task<SearchNotesResponse> SearchNotesAsync(string userId, string query, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

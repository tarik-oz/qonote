using Qonote.Core.Application.Features.Sidebar.GetSidebar;
using Qonote.Core.Application.Features.Notes.SearchNotes;

namespace Qonote.Core.Application.Abstractions.Queries;

public interface INoteQueries
{
    Task<List<NoteListItemDto>> GetOrderedNotesForGroupAsync(int groupId, string userId, int? offset, int? limit, CancellationToken cancellationToken);
    Task<List<NoteListItemDto>> GetOrderedUngroupedNotesAsync(string userId, int? offset, int? limit, CancellationToken cancellationToken);
    Task<int> CountActiveNotesAsync(string userId, CancellationToken cancellationToken);
    // Count notes created in [startUtc, endUtc), including deleted notes (to prevent delete-refund gaming)
    Task<int> CountNotesCreatedInPeriodAsync(string userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken);
    Task<SearchNotesResponse> SearchNotesAsync(string userId, string query, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

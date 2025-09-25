using MediatR;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;

namespace Qonote.Core.Application.Features.Notes.ListFlatNotes;

public sealed class ListFlatNotesQueryHandler : IRequestHandler<ListFlatNotesQuery, List<NoteListItemDto>>
{
    private readonly INoteQueries _noteQueries;
    private readonly ICurrentUserService _currentUser;

    public ListFlatNotesQueryHandler(INoteQueries noteQueries, ICurrentUserService currentUser)
    {
        _noteQueries = noteQueries;
        _currentUser = currentUser;
    }

    public async Task<List<NoteListItemDto>> Handle(ListFlatNotesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule
        var notes = await _noteQueries.GetOrderedUngroupedNotesAsync(userId, request.Limit, request.Offset, cancellationToken);
        return notes;
    }
}

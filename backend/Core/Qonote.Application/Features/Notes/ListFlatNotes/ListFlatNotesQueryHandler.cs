using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;

namespace Qonote.Core.Application.Features.Notes.ListFlatNotes;

public sealed class ListFlatNotesQueryHandler : IRequestHandler<ListFlatNotesQuery, List<NoteListItemDto>>
{
    private readonly INoteQueries _noteQueries;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ListFlatNotesQueryHandler> _logger;

    public ListFlatNotesQueryHandler(INoteQueries noteQueries, ICurrentUserService currentUser, ILogger<ListFlatNotesQueryHandler> logger)
    {
        _noteQueries = noteQueries;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<NoteListItemDto>> Handle(ListFlatNotesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule
        var notes = await _noteQueries.GetOrderedUngroupedNotesAsync(userId, request.Limit, request.Offset, cancellationToken);
        _logger.LogInformation("Flat notes listed. UserId={UserId} Count={Count} Limit={Limit} Offset={Offset}", userId, notes.Count, request.Limit, request.Offset);
        return notes;
    }
}

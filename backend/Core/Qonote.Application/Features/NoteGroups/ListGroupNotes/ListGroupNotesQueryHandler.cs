using AutoMapper;
using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.NoteGroups.ListGroupNotes;

public sealed class ListGroupNotesQueryHandler : IRequestHandler<ListGroupNotesQuery, List<NoteListItemDto>>
{
    private readonly IReadRepository<NoteGroup, int> _groupReader;
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly ICurrentUserService _currentUser;
    private readonly INoteQueries _noteQueries;

    public ListGroupNotesQueryHandler(
        IReadRepository<NoteGroup, int> groupReader,
        IReadRepository<Note, int> noteReader,
        ICurrentUserService currentUser,
        INoteQueries noteQueries)
    {
        _groupReader = groupReader;
        _noteReader = noteReader;
        _currentUser = currentUser;
        _noteQueries = noteQueries;
    }

    public async Task<List<NoteListItemDto>> Handle(ListGroupNotesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule
        var group = await _groupReader.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null || group.UserId != userId)
        {
            throw new NotFoundException($"Note group {request.GroupId} not found.");
        }

        var notes = await _noteQueries.GetOrderedNotesForGroupAsync(request.GroupId, userId, request.Offset, request.Limit, cancellationToken);

        return notes;
    }
}

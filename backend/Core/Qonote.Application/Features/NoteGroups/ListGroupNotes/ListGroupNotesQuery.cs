using MediatR;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.NoteGroups.ListGroupNotes;

public sealed record ListGroupNotesQuery(
    int GroupId,
    int? Limit = null,
    int? Offset = null
) : IRequest<List<NoteListItemDto>>, IAuthenticatedRequest;

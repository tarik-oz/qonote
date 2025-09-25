using MediatR;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Notes.ListFlatNotes;

public sealed record ListFlatNotesQuery(
    int? Limit = null,
    int? Offset = null
) : IRequest<List<NoteListItemDto>>, IAuthenticatedRequest;

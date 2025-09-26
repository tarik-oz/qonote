using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.NoteGroups.Reorder;

public sealed record ReorderNoteGroupsCommand(
    List<Notes.Reorder.ReorderItem> Items
) : IRequest, IAuthenticatedRequest;

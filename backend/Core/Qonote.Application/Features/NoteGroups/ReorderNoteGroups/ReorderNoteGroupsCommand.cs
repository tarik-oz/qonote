using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.NoteGroups.ReorderNoteGroups;

public sealed record ReorderNoteGroupsCommand(
    List<Notes.ReorderNotes.NoteReorderItem> Items
) : IRequest, IAuthenticatedRequest;

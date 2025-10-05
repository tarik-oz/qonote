using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.NoteGroups.ReorderNoteGroups;

public sealed record ReorderNoteGroupsCommand(
    List<Notes.ReorderNotes.ReorderItem> Items
) : IRequest, IAuthenticatedRequest;

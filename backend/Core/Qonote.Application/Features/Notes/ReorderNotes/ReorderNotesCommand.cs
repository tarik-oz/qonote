using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Notes.ReorderNotes;

public sealed record ReorderNotesCommand(
    List<NoteReorderItem> Items,
    int? NoteGroupId = null // null means ungrouped scope
) : IRequest, IAuthenticatedRequest;

public sealed class NoteReorderItem
{
    public int Id { get; set; }
    public int Order { get; set; }
}

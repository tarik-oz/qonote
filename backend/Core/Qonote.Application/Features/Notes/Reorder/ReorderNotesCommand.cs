using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Notes.Reorder;

public sealed record ReorderNotesCommand(
    List<ReorderItem> Items
) : IRequest, IAuthenticatedRequest;

public sealed class ReorderItem
{
    public int Id { get; set; }
    public int Order { get; set; }
}

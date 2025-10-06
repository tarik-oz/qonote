using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Sections.SetUiStateBatch;

public sealed record SetSectionUiStateBatchCommand(
    int NoteId,
    List<SetItem> Items
) : IRequest, IAuthenticatedRequest;

public sealed class SetItem
{
    public int SectionId { get; set; }
    public bool IsCollapsed { get; set; }
}



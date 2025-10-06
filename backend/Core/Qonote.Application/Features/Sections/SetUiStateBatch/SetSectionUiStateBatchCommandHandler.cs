using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;

namespace Qonote.Core.Application.Features.Sections.SetUiStateBatch;

public sealed class SetSectionUiStateBatchCommandHandler : IRequestHandler<SetSectionUiStateBatchCommand>
{
    private readonly ISectionUiStateStore _store;
    private readonly ICurrentUserService _currentUser;

    public SetSectionUiStateBatchCommandHandler(ISectionUiStateStore store, ICurrentUserService currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public async Task Handle(SetSectionUiStateBatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        await _store.SetCollapsedBatchAsync(userId, request.NoteId,
            request.Items.Select(i => (i.SectionId, i.IsCollapsed)), cancellationToken);
    }
}



using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;

namespace Qonote.Core.Application.Features.Sections.SetSectionUiStateBatch;

public sealed class SetSectionUiStateBatchCommandHandler : IRequestHandler<SetSectionUiStateBatchCommand>
{
    private readonly ISectionUiStateStore _store;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SetSectionUiStateBatchCommandHandler> _logger;

    public SetSectionUiStateBatchCommandHandler(ISectionUiStateStore store, ICurrentUserService currentUser, ILogger<SetSectionUiStateBatchCommandHandler> logger)
    {
        _store = store;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(SetSectionUiStateBatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        await _store.SetCollapsedBatchAsync(userId, request.NoteId,
            request.Items.Select(i => (i.SectionId, i.IsCollapsed)), cancellationToken);
        _logger.LogInformation("Section UI state batch set for Note {NoteId} items={Count} by {UserId}", request.NoteId, request.Items?.Count ?? 0, userId);
    }
}

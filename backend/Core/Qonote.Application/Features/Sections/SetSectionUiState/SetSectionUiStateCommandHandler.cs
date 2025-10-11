using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;

namespace Qonote.Core.Application.Features.Sections.SetSectionUiState;

public sealed class SetSectionUiStateCommandHandler : IRequestHandler<SetSectionUiStateCommand>
{
    private readonly ISectionUiStateStore _store;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SetSectionUiStateCommandHandler> _logger;

    public SetSectionUiStateCommandHandler(ISectionUiStateStore store, ICurrentUserService currentUser, ILogger<SetSectionUiStateCommandHandler> logger)
    {
        _store = store;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(SetSectionUiStateCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;

        await _store.SetCollapsedAsync(userId, request.SectionId, request.IsCollapsed, cancellationToken);
        _logger.LogInformation("Section UI state set {SectionId} collapsed={Collapsed} by {UserId}", request.SectionId, request.IsCollapsed, userId);
    }
}

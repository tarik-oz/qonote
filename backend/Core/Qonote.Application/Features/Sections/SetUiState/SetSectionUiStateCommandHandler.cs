using MediatR;
using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Sections.SetUiState;

public sealed class SetSectionUiStateCommandHandler : IRequestHandler<SetSectionUiStateCommand>
{
    private readonly ISectionUiStateStore _store;
    private readonly ICurrentUserService _currentUser;

    public SetSectionUiStateCommandHandler(ISectionUiStateStore store, ICurrentUserService currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public async Task Handle(SetSectionUiStateCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;

        await _store.SetCollapsedAsync(userId, request.SectionId, request.IsCollapsed, cancellationToken);
    }
}



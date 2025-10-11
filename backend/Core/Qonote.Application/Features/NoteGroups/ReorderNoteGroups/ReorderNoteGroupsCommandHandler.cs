using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.NoteGroups.ReorderNoteGroups;

public sealed class ReorderNoteGroupsCommandHandler : IRequestHandler<ReorderNoteGroupsCommand>
{
    private readonly IReadRepository<NoteGroup, int> _groupReader;
    private readonly IWriteRepository<NoteGroup, int> _groupWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ReorderNoteGroupsCommandHandler> _logger;

    public ReorderNoteGroupsCommandHandler(
        IReadRepository<NoteGroup, int> groupReader,
        IWriteRepository<NoteGroup, int> groupWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<ReorderNoteGroupsCommandHandler> logger)
    {
        _groupReader = groupReader;
        _groupWriter = groupWriter;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(ReorderNoteGroupsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        var ids = request.Items.Select(i => i.Id).ToHashSet();
        var groups = await _groupReader.GetAllAsync(g => ids.Contains(g.Id), cancellationToken);
        if (groups.Any(g => g.UserId != userId))
        {
            throw new NotFoundException("One or more groups not found.");
        }
        var byId = groups.ToDictionary(g => g.Id);
        foreach (var item in request.Items)
        {
            if (byId.TryGetValue(item.Id, out var group))
            {
                group.Order = item.Order;
                _groupWriter.Update(group);
            }
        }
        // Normalize all groups for the user to contiguous order 0..n-1 based on Order then Id
        var allGroups = await _groupReader.GetAllAsync(g => g.UserId == userId, cancellationToken);
        var ordered = allGroups.OrderBy(g => g.Order).ThenBy(g => g.Id).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].Order != i)
            {
                ordered[i].Order = i;
                _groupWriter.Update(ordered[i]);
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Note groups reordered and normalized by {UserId}", userId);
    }
}

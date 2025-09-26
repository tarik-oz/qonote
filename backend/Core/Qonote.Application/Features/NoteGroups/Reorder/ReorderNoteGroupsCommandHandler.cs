using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.NoteGroups.Reorder;

public sealed class ReorderNoteGroupsCommandHandler : IRequestHandler<ReorderNoteGroupsCommand>
{
    private readonly IReadRepository<NoteGroup, int> _groupReader;
    private readonly IWriteRepository<NoteGroup, int> _groupWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ReorderNoteGroupsCommandHandler(
        IReadRepository<NoteGroup, int> groupReader,
        IWriteRepository<NoteGroup, int> groupWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _groupReader = groupReader;
        _groupWriter = groupWriter;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ReorderNoteGroupsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        var ids = request.Items.Select(i => i.Id).ToHashSet();
        var groups = await _groupReader.GetAllAsync(g => ids.Contains(g.Id) && g.UserId == userId, cancellationToken);
        var byId = groups.ToDictionary(g => g.Id);
        foreach (var item in request.Items)
        {
            if (byId.TryGetValue(item.Id, out var group))
            {
                group.Order = item.Order;
                _groupWriter.Update(group);
            }
        }
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes.Reorder;

public sealed class ReorderNotesCommandHandler : IRequestHandler<ReorderNotesCommand>
{
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly IWriteRepository<Note, int> _noteWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ReorderNotesCommandHandler(
        IReadRepository<Note, int> noteReader,
        IWriteRepository<Note, int> noteWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _noteReader = noteReader;
        _noteWriter = noteWriter;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ReorderNotesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule
        var ids = request.Items.Select(i => i.Id).ToHashSet();
        // Fetch notes in one go (repository has only GetAllAsync, use it)
        var notes = await _noteReader.GetAllAsync(n => ids.Contains(n.Id) && n.UserId == userId, cancellationToken);
        var byId = notes.ToDictionary(n => n.Id);
        foreach (var item in request.Items)
        {
            if (byId.TryGetValue(item.Id, out var note))
            {
                note.Order = item.Order;
                _noteWriter.Update(note);
            }
        }
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

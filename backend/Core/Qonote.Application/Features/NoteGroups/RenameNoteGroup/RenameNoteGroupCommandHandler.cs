using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.NoteGroups.RenameNoteGroup;

public sealed class RenameNoteGroupCommandHandler : IRequestHandler<RenameNoteGroupCommand>
{
    private readonly IReadRepository<NoteGroup, int> _reader;
    private readonly IWriteRepository<NoteGroup, int> _writer;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RenameNoteGroupCommandHandler(
        IReadRepository<NoteGroup, int> reader,
        IWriteRepository<NoteGroup, int> writer,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _reader = reader;
        _writer = writer;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(RenameNoteGroupCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        var group = await _reader.GetByIdAsync(request.Id, cancellationToken);
        if (group is null || group.UserId != userId)
        {
            throw new NotFoundException($"Note group {request.Id} not found.");
        }

        group.Name = request.Title.Trim();
        _writer.Update(group);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

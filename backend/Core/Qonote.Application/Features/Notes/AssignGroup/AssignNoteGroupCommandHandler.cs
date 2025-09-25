using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes.AssignGroup;

public sealed class AssignNoteGroupCommandHandler : IRequestHandler<AssignNoteGroupCommand>
{
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly IReadRepository<NoteGroup, int> _groupReader;
    private readonly IWriteRepository<Note, int> _noteWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AssignNoteGroupCommandHandler(
        IReadRepository<Note, int> noteReader,
        IReadRepository<NoteGroup, int> groupReader,
        IWriteRepository<Note, int> noteWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _noteReader = noteReader;
        _groupReader = groupReader;
        _noteWriter = noteWriter;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(AssignNoteGroupCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule

        var note = await _noteReader.GetByIdAsync(request.NoteId, cancellationToken);
        if (note is null || note.UserId != userId)
        {
            throw new NotFoundException($"Note {request.NoteId} not found.");
        }

        if (request.NoteGroupId.HasValue)
        {
            var group = await _groupReader.GetByIdAsync(request.NoteGroupId.Value, cancellationToken);
            if (group is null || group.UserId != userId)
            {
                throw new NotFoundException($"Note group {request.NoteGroupId.Value} not found.");
            }
        }

        note.NoteGroupId = request.NoteGroupId; // can be null to unassign
        _noteWriter.Update(note);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes.DeleteNote;

public sealed class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand>
{
    private readonly IReadRepository<Note, int> _reader;
    private readonly IWriteRepository<Note, int> _writer;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteNoteCommandHandler(
        IReadRepository<Note, int> reader,
        IWriteRepository<Note, int> writer,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _reader = reader;
        _writer = writer;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // ensured by IAuthenticatedRequest
        var note = await _reader.GetByIdAsync(request.Id, cancellationToken);
        if (note is null || note.UserId != userId)
        {
            throw new NotFoundException($"Note {request.Id} not found.");
        }

        _writer.Delete(note); // soft delete for note
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

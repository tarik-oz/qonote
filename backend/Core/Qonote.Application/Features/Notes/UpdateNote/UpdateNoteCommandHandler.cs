using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes.UpdateNote;

public sealed class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand>
{
    private readonly IReadRepository<Note, int> _reader;
    private readonly IWriteRepository<Note, int> _writer;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateNoteCommandHandler(
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

    public async Task Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // ensured by IAuthenticatedRequest
        var note = await _reader.GetByIdAsync(request.Id, cancellationToken);
        if (note is null || note.UserId != userId)
        {
            throw new NotFoundException($"Note {request.Id} not found.");
        }

        if (request.CustomTitle is not null) note.CustomTitle = request.CustomTitle.Trim();
        if (request.IsPublic is not null) note.IsPublic = request.IsPublic.Value;
        if (request.UserLink1 is not null) note.UserLink1 = request.UserLink1;
        if (request.UserLink2 is not null) note.UserLink2 = request.UserLink2;
        if (request.UserLink3 is not null) note.UserLink3 = request.UserLink3;

        _writer.Update(note);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

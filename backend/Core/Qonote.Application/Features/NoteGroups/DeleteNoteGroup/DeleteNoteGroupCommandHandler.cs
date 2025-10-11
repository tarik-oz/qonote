using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.NoteGroups.DeleteNoteGroup;

public sealed class DeleteNoteGroupCommandHandler : IRequestHandler<DeleteNoteGroupCommand>
{
    private readonly IReadRepository<NoteGroup, int> _groupReader;
    private readonly IWriteRepository<NoteGroup, int> _groupWriter;
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly IWriteRepository<Note, int> _noteWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DeleteNoteGroupCommandHandler> _logger;

    public DeleteNoteGroupCommandHandler(
        IReadRepository<NoteGroup, int> groupReader,
        IWriteRepository<NoteGroup, int> groupWriter,
        IReadRepository<Note, int> noteReader,
        IWriteRepository<Note, int> noteWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<DeleteNoteGroupCommandHandler> logger)
    {
        _groupReader = groupReader;
        _groupWriter = groupWriter;
        _noteReader = noteReader;
        _noteWriter = noteWriter;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(DeleteNoteGroupCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // ensured by IAuthenticatedRequest
        var group = await _groupReader.GetByIdAsync(request.Id, cancellationToken);
        if (group is null || group.UserId != userId)
        {
            throw new NotFoundException($"Note group {request.Id} not found.");
        }

        // Unassign notes from this group (set NoteGroupId = null), keeping notes intact.
        var notesInGroup = await _noteReader.GetAllAsync(n => n.NoteGroupId == request.Id && n.UserId == userId, cancellationToken);
        foreach (var note in notesInGroup)
        {
            note.NoteGroupId = null;
            _noteWriter.Update(note);
        }

        _groupWriter.Delete(group); // soft delete group itself
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Note group deleted {GroupId} by {UserId}", group.Id, userId);
    }
}

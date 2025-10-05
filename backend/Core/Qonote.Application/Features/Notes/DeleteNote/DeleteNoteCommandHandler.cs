using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Application.Abstractions.Storage;

namespace Qonote.Core.Application.Features.Notes.DeleteNote;

public sealed class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand>
{
    private readonly IReadRepository<Note, int> _reader;
    private readonly IWriteRepository<Note, int> _writer;
    private readonly IReadRepository<Section, int> _sectionReader;
    private readonly IWriteRepository<Section, int> _sectionWriter;
    private readonly IReadRepository<Block, Guid> _blockReader;
    private readonly IWriteRepository<Block, Guid> _blockWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public DeleteNoteCommandHandler(
        IReadRepository<Note, int> reader,
        IWriteRepository<Note, int> writer,
        IReadRepository<Section, int> sectionReader,
        IWriteRepository<Section, int> sectionWriter,
        IReadRepository<Block, Guid> blockReader,
        IWriteRepository<Block, Guid> blockWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage)
    {
        _reader = reader;
        _writer = writer;
        _sectionReader = sectionReader;
        _sectionWriter = sectionWriter;
        _blockReader = blockReader;
        _blockWriter = blockWriter;
        _uow = uow;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // ensured by IAuthenticatedRequest
        var note = await _reader.GetByIdAsync(request.Id, cancellationToken);
        if (note is null || note.UserId != userId)
        {
            throw new NotFoundException($"Note {request.Id} not found.");
        }

        // Cascade soft delete: sections and blocks
        var sections = await _sectionReader.GetAllAsync(s => s.NoteId == note.Id, cancellationToken);
        foreach (var section in sections)
        {
            var blocks = await _blockReader.GetAllAsync(b => b.SectionId == section.Id, cancellationToken);
            foreach (var block in blocks)
            {
                _blockWriter.Delete(block);
            }
            _sectionWriter.Delete(section);
        }

        // Finally soft delete the note
        _writer.Delete(note);
        await _uow.SaveChangesAsync(cancellationToken);

        // Best-effort: delete thumbnail blob
        if (!string.IsNullOrWhiteSpace(note.ThumbnailUrl))
        {
            await _fileStorage.DeleteAsync(note.ThumbnailUrl, "thumbnails");
        }
    }
}

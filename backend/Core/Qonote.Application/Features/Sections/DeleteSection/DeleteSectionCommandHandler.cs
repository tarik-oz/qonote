using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Sections.DeleteSection;

public sealed class DeleteSectionCommandHandler : IRequestHandler<DeleteSectionCommand>
{
    private readonly IReadRepository<Section, int> _sectionReader;
    private readonly IReadRepository<Block, Guid> _blockReader;
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly IWriteRepository<Section, int> _sectionWriter;
    private readonly IWriteRepository<Block, Guid> _blockWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteSectionCommandHandler(
        IReadRepository<Section, int> sectionReader,
        IReadRepository<Block, Guid> blockReader,
        IReadRepository<Note, int> noteReader,
        IWriteRepository<Section, int> sectionWriter,
        IWriteRepository<Block, Guid> blockWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _sectionReader = sectionReader;
        _blockReader = blockReader;
        _noteReader = noteReader;
        _sectionWriter = sectionWriter;
        _blockWriter = blockWriter;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _sectionReader.GetByIdAsync(request.Id, cancellationToken);
        if (section is null)
        {
            throw new NotFoundException($"Section {request.Id} not found.");
        }

        var userId = _currentUser.UserId!;
        var note = await _noteReader.GetByIdAsync(section.NoteId, cancellationToken);
        if (note is null || note.UserId != userId)
        {
            throw new NotFoundException($"Section {request.Id} not found.");
        }

        // Soft delete section and its blocks (MVP: assume single block, but delete all to be safe)
        var blocks = await _blockReader.GetAllAsync(b => b.SectionId == section.Id, cancellationToken);
        foreach (var b in blocks)
        {
            _blockWriter.Delete(b);
        }

        _sectionWriter.Delete(section);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Sections.CreateSection;

public sealed class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, int>
{
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly IReadRepository<Section, int> _sectionReader;
    private readonly IWriteRepository<Section, int> _sectionWriter;
    private readonly IWriteRepository<Block, Guid> _blockWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateSectionCommandHandler(
        IReadRepository<Note, int> noteReader,
        IReadRepository<Section, int> sectionReader,
        IWriteRepository<Section, int> sectionWriter,
        IWriteRepository<Block, Guid> blockWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _noteReader = noteReader;
        _sectionReader = sectionReader;
        _sectionWriter = sectionWriter;
        _blockWriter = blockWriter;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        var note = await _noteReader.GetByIdAsync(request.NoteId, cancellationToken);
        if (note is null || note.UserId != userId)
        {
            throw new NotFoundException($"Note {request.NoteId} not found.");
        }

        // Append to end
        var existing = await _sectionReader.GetAllAsync(s => s.NoteId == note.Id, cancellationToken);
        var nextOrder = existing.Count == 0 ? 0 : existing.Max(s => s.Order) + 1;

        var section = new Section
        {
            NoteId = note.Id,
            Title = request.Title?.Trim() ?? string.Empty,
            Type = request.Type ?? SectionType.GeneralNote,
            StartTime = request.StartTime ?? TimeSpan.Zero,
            EndTime = request.EndTime ?? TimeSpan.Zero,
            Order = nextOrder
        };

        await _sectionWriter.AddAsync(section, cancellationToken);

        // Create the single block for this section (empty content)
        var block = new Block
        {
            Section = section,
            Content = string.Empty,
            Type = BlockType.Paragraph,
            Order = 0
        };
        await _blockWriter.AddAsync(block, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
        return section.Id;
    }
}

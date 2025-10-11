using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<DeleteSectionCommandHandler> _logger;

    public DeleteSectionCommandHandler(
        IReadRepository<Section, int> sectionReader,
        IReadRepository<Block, Guid> blockReader,
        IReadRepository<Note, int> noteReader,
        IWriteRepository<Section, int> sectionWriter,
        IWriteRepository<Block, Guid> blockWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<DeleteSectionCommandHandler> logger)
    {
        _sectionReader = sectionReader;
        _blockReader = blockReader;
        _noteReader = noteReader;
        _sectionWriter = sectionWriter;
        _blockWriter = blockWriter;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
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

        // Only Timestamped sections are deletable; fixed sections (VideoInfo/General) are not
        if (section.Type != Core.Domain.Enums.SectionType.Timestamped)
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Id", "Only Timestamped sections can be deleted.") });
        }

        // Soft delete blocks first
        var blocks = await _blockReader.GetAllAsync(b => b.SectionId == section.Id, cancellationToken);
        foreach (var b in blocks)
        {
            _blockWriter.Delete(b);
        }

        // Timeline adjustment for Timestamped: merge into left or pull right
        if (section.Type == Core.Domain.Enums.SectionType.Timestamped)
        {
            var siblings = await _sectionReader.GetAllAsync(s => s.NoteId == section.NoteId && s.Type == Core.Domain.Enums.SectionType.Timestamped, cancellationToken);
            var ordered = siblings.Where(s => s.Id != section.Id).OrderBy(s => s.Order).ToList();
            var left = ordered.LastOrDefault(s => s.Order < section.Order);
            var right = ordered.FirstOrDefault(s => s.Order > section.Order);
            if (left is not null)
            {
                left.EndTime = section.EndTime;
                _sectionWriter.Update(left);
            }
            else if (right is not null)
            {
                right.StartTime = section.StartTime;
                _sectionWriter.Update(right);
            }
        }

        _sectionWriter.Delete(section);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Section deleted {SectionId} for Note {NoteId} by {UserId}", section.Id, note.Id, userId);
    }
}

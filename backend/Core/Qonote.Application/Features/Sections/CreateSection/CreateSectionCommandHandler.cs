using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Sections._Shared;
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
    private readonly ILogger<CreateSectionCommandHandler> _logger;

    public CreateSectionCommandHandler(
        IReadRepository<Note, int> noteReader,
        IReadRepository<Section, int> sectionReader,
        IWriteRepository<Section, int> sectionWriter,
        IWriteRepository<Block, Guid> blockWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<CreateSectionCommandHandler> logger)
    {
        _noteReader = noteReader;
        _sectionReader = sectionReader;
        _sectionWriter = sectionWriter;
        _blockWriter = blockWriter;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<int> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        var note = await _noteReader.GetByIdAsync(request.NoteId, cancellationToken);
        if (note is null || note.UserId != userId)
        {
            throw new NotFoundException($"Note {request.NoteId} not found.");
        }

        // Fetch existing sections for the note
        var existing = await _sectionReader.GetAllAsync(s => s.NoteId == note.Id, cancellationToken);

        // Only allow creating Timestamped via this endpoint for timeline
        var type = request.Type ?? SectionType.Timestamped;
        if (type != SectionType.Timestamped)
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Type", "Only Timestamped sections can be created via this endpoint.")]);
        }

        // Timestamped insert orchestration
        var insertedId = await InsertTimestampedAsync(note, existing, request, userId, cancellationToken);
        return insertedId;
    }

    private async Task<int> InsertTimestampedAsync(Note note, IEnumerable<Section> existing, CreateSectionCommand request, string userId, CancellationToken cancellationToken)
    {
        var minLen = SectionTimelineRules.MinLength;
        var tsSections = existing
            .Where(s => s.Type == SectionType.Timestamped)
            .OrderBy(s => s.Order)
            .ToList();

        if (tsSections.Count == 0)
        {
            return await InsertFirstAsync(note, request, userId, minLen, cancellationToken);
        }

        if (request.StartTime.HasValue)
        {
            var id = await TryInsertWithHintAsync(note, tsSections, request, userId, minLen, cancellationToken);
            if (id.HasValue) return id.Value;
        }

        return await AppendAsync(note, tsSections, request, userId, minLen, cancellationToken);
    }

    private async Task<int> InsertFirstAsync(Note note, CreateSectionCommand request, string userId, TimeSpan minLen, CancellationToken cancellationToken)
    {
        var start = note.VideoDuration > minLen ? note.VideoDuration - minLen : TimeSpan.Zero;
        var section = new Section
        {
            NoteId = note.Id,
            Title = request.Title?.Trim() ?? string.Empty,
            Type = SectionType.Timestamped,
            StartTime = start,
            EndTime = note.VideoDuration,
            Order = 0
        };
        await _sectionWriter.AddAsync(section, cancellationToken);
        var block = new Block { Section = section, Content = string.Empty, Type = BlockType.Paragraph, Order = 0 };
        await _blockWriter.AddAsync(block, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Section created (first) {SectionId} for Note {NoteId} by {UserId}", section.Id, note.Id, userId);
        return section.Id;
    }

    private async Task<int?> TryInsertWithHintAsync(Note note, List<Section> tsSections, CreateSectionCommand request, string userId, TimeSpan minLen, CancellationToken cancellationToken)
    {
        var hint = request.StartTime!.Value;
        var right = tsSections.FirstOrDefault(s => s.StartTime >= hint);
        if (right is null)
        {
            return null;
        }

        var rightIndex = tsSections.FindIndex(s => s.Id == right.Id);
        var leftIndex = rightIndex - 1;
        if (leftIndex < 0)
        {
            // Insert at beginning
            var first = tsSections[0];
            var firstLen = first.EndTime - first.StartTime;
            if (firstLen < minLen * 2)
            {
                throw new ValidationException([new FluentValidation.Results.ValidationFailure("StartTime", "Not enough duration at the beginning to insert a new section of minimum length.")]);
            }

            var oldStart = first.StartTime;
            first.StartTime = first.StartTime + minLen;
            _sectionWriter.Update(first);

            // Shift orders of all existing sections by +1
            for (int i = 0; i < tsSections.Count; i++)
            {
                tsSections[i].Order += 1;
                _sectionWriter.Update(tsSections[i]);
            }

            var newSectionBegin = new Section
            {
                NoteId = note.Id,
                Title = request.Title?.Trim() ?? string.Empty,
                Type = SectionType.Timestamped,
                StartTime = oldStart,
                EndTime = oldStart + minLen,
                Order = first.Order - 1
            };
            await _sectionWriter.AddAsync(newSectionBegin, cancellationToken);
            var newBlockBeg = new Block { Section = newSectionBegin, Content = string.Empty, Type = BlockType.Paragraph, Order = 0 };
            await _blockWriter.AddAsync(newBlockBeg, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Section created (begin) {SectionId} for Note {NoteId} by {UserId}", newSectionBegin.Id, note.Id, userId);
            return newSectionBegin.Id;
        }

        // borrow from left chain to create a gap of minLen ending at right.StartTime
        var borrowRemaining = minLen;
        for (int i = leftIndex; i >= 0 && borrowRemaining > TimeSpan.Zero; i--)
        {
            var s = tsSections[i];
            var length = s.EndTime - s.StartTime;
            var canBorrow = length - minLen;
            if (canBorrow <= TimeSpan.Zero) continue;
            var take = canBorrow < borrowRemaining ? canBorrow : borrowRemaining;
            s.EndTime -= take;
            borrowRemaining -= take;
            _sectionWriter.Update(s);
        }

        if (borrowRemaining > TimeSpan.Zero)
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("StartTime", "Not enough duration to insert a new section of minimum length.")]);
        }

        var newStart = tsSections[leftIndex].EndTime; // after shrinking, this is new gap start
        var newEnd = right.StartTime; // keep right start as original boundary

        // shift orders for right and subsequent
        for (int i = rightIndex; i < tsSections.Count; i++)
        {
            tsSections[i].Order += 1;
            _sectionWriter.Update(tsSections[i]);
        }

        var newSectionMid = new Section
        {
            NoteId = note.Id,
            Title = request.Title?.Trim() ?? string.Empty,
            Type = SectionType.Timestamped,
            StartTime = newStart,
            EndTime = newEnd,
            // occupy right's previous slot (right was shifted by +1 just above)
            Order = right.Order - 1
        };
        await _sectionWriter.AddAsync(newSectionMid, cancellationToken);
        var newBlockMid = new Block { Section = newSectionMid, Content = string.Empty, Type = BlockType.Paragraph, Order = 0 };
        await _blockWriter.AddAsync(newBlockMid, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Section created (mid) {SectionId} before {RightSectionId} for Note {NoteId} by {UserId}", newSectionMid.Id, right.Id, note.Id, userId);
        return newSectionMid.Id;
    }

    private async Task<int> AppendAsync(Note note, List<Section> tsSections, CreateSectionCommand request, string userId, TimeSpan minLen, CancellationToken cancellationToken)
    {
        var borrowRemaining = minLen;
        for (int i = tsSections.Count - 1; i >= 0 && borrowRemaining > TimeSpan.Zero; i--)
        {
            var s = tsSections[i];
            var length = s.EndTime - s.StartTime;
            var canBorrow = length - minLen;
            if (canBorrow <= TimeSpan.Zero) continue;
            var take = canBorrow < borrowRemaining ? canBorrow : borrowRemaining;
            s.EndTime -= take;
            borrowRemaining -= take;
            _sectionWriter.Update(s);
        }

        if (borrowRemaining > TimeSpan.Zero)
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("StartTime", "Not enough duration to insert a new section of minimum length.")]);
        }

        var newStart = note.VideoDuration - minLen;
        var appendedSection = new Section
        {
            NoteId = note.Id,
            Title = request.Title?.Trim() ?? string.Empty,
            Type = SectionType.Timestamped,
            StartTime = newStart,
            EndTime = note.VideoDuration,
            Order = tsSections.Max(s => s.Order) + 1
        };
        await _sectionWriter.AddAsync(appendedSection, cancellationToken);
        var newBlockEnd = new Block { Section = appendedSection, Content = string.Empty, Type = BlockType.Paragraph, Order = 0 };
        await _blockWriter.AddAsync(newBlockEnd, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Section created (append) {SectionId} for Note {NoteId} by {UserId}", appendedSection.Id, note.Id, userId);
        return appendedSection.Id;
    }
}

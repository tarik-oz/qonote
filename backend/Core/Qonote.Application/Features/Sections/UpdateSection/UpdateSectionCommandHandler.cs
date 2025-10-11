using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Features.Sections._Shared;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Sections.UpdateSection;

public sealed class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand>
{
    private readonly IReadRepository<Section, int> _sectionReader;
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly IWriteRepository<Section, int> _sectionWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateSectionCommandHandler> _logger;

    public UpdateSectionCommandHandler(
        IReadRepository<Section, int> sectionReader,
        IReadRepository<Note, int> noteReader,
        IWriteRepository<Section, int> sectionWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<UpdateSectionCommandHandler> logger)
    {
        _sectionReader = sectionReader;
        _noteReader = noteReader;
        _sectionWriter = sectionWriter;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
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

        // Enforce per-type rules: VideoInfo immutable; General allows Title only; Timestamped allows Title/Start/End
        var isTimestamped = section.Type == Core.Domain.Enums.SectionType.Timestamped;
        var isVideoInfo = section.Type == Core.Domain.Enums.SectionType.VideoInfo;

        if (isVideoInfo)
        {
            // Ignore all updates for VideoInfo to keep invariants
        }
        else if (isTimestamped)
        {
            if (request.Title is not null) section.Title = request.Title.Trim();

            var newStart = request.StartTime ?? section.StartTime;
            var newEnd = request.EndTime ?? section.EndTime;

            // Basic consistency: Start < End
            if (newStart >= newEnd)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure("StartTime", "StartTime must be less than EndTime."),
                    new FluentValidation.Results.ValidationFailure("EndTime", "EndTime must be greater than StartTime.")
                });
            }

            // Minimum length constraint
            if (newEnd - newStart < SectionTimelineRules.MinLength)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure("EndTime", $"Section length must be at least {SectionTimelineRules.MinLength.TotalSeconds} seconds."),
                });
            }

            // Bounds within [0, VideoDuration]
            if (newStart < TimeSpan.Zero || newEnd > note.VideoDuration)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure("StartTime", "StartTime must be within the video duration."),
                    new FluentValidation.Results.ValidationFailure("EndTime", "EndTime must be within the video duration.")
                });
            }

            // No overlap with siblings (adjacent is allowed: touching endpoints is ok)
            var siblings = await _sectionReader.GetAllAsync(
                s => s.NoteId == section.NoteId && s.Type == Core.Domain.Enums.SectionType.Timestamped && s.Id != section.Id,
                cancellationToken);
            var overlaps = siblings.Any(s => newStart < s.EndTime && newEnd > s.StartTime);
            if (overlaps)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure("StartTime", "Updated time range overlaps with another section."),
                    new FluentValidation.Results.ValidationFailure("EndTime", "Updated time range overlaps with another section.")
                });
            }

            section.StartTime = newStart;
            section.EndTime = newEnd;
        }
        else
        {
            // GeneralNote: Title only
            if (request.Title is not null) section.Title = request.Title.Trim();
        }

        _sectionWriter.Update(section);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Section updated {SectionId} for Note {NoteId} by {UserId}", section.Id, note.Id, _currentUser.UserId);
    }
}

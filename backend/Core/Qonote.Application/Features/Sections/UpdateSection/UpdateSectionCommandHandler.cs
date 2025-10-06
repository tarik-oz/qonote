using MediatR;
using Qonote.Core.Application.Abstractions.Data;
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

    public UpdateSectionCommandHandler(
        IReadRepository<Section, int> sectionReader,
        IReadRepository<Note, int> noteReader,
        IWriteRepository<Section, int> sectionWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _sectionReader = sectionReader;
        _noteReader = noteReader;
        _sectionWriter = sectionWriter;
        _uow = uow;
        _currentUser = currentUser;
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
            if (request.StartTime is not null) section.StartTime = request.StartTime.Value;
            if (request.EndTime is not null) section.EndTime = request.EndTime.Value;
        }
        else
        {
            // GeneralNote: Title only
            if (request.Title is not null) section.Title = request.Title.Trim();
        }

        _sectionWriter.Update(section);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

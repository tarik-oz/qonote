using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Sections.ReorderSections;

public sealed class ReorderSectionsCommandHandler : IRequestHandler<ReorderSectionsCommand>
{
    private readonly IReadRepository<Section, int> _sectionReader;
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly IWriteRepository<Section, int> _sectionWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ReorderSectionsCommandHandler> _logger;

    public ReorderSectionsCommandHandler(
        IReadRepository<Section, int> sectionReader,
        IReadRepository<Note, int> noteReader,
        IWriteRepository<Section, int> sectionWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<ReorderSectionsCommandHandler> logger)
    {
        _sectionReader = sectionReader;
        _noteReader = noteReader;
        _sectionWriter = sectionWriter;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(ReorderSectionsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        var note = await _noteReader.GetByIdAsync(request.NoteId, cancellationToken);
        if (note is null || note.UserId != userId)
        {
            throw new NotFoundException($"Note {request.NoteId} not found.");
        }

        var ids = request.Items.Select(i => i.Id).ToHashSet();
        var noteSections = await _sectionReader.GetAllAsync(s => s.NoteId == request.NoteId, cancellationToken);

        // Apply incoming orders to targeted timestamped sections only
        foreach (var item in request.Items)
        {
            var section = noteSections.FirstOrDefault(s => s.Id == item.Id);
            if (section is null) continue;
            if (section.Type != SectionType.Timestamped) continue; // fixed sections not reorderable
            section.Order = item.Order;
            _sectionWriter.Update(section);
        }

        // Now normalize all timestamped sections in the note to contiguous 0..n-1 order based on current Order then Id
        var timestamped = noteSections.Where(s => s.Type == SectionType.Timestamped)
            .OrderBy(s => s.Order).ThenBy(s => s.Id)
            .ToList();
        for (int i = 0; i < timestamped.Count; i++)
        {
            if (timestamped[i].Order != i)
            {
                timestamped[i].Order = i;
                _sectionWriter.Update(timestamped[i]);
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Sections reordered and normalized for Note {NoteId} by {UserId}", request.NoteId, userId);
    }
}

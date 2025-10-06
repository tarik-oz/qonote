using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
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

    public ReorderSectionsCommandHandler(
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

    public async Task Handle(ReorderSectionsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!;
        var note = await _noteReader.GetByIdAsync(request.NoteId, cancellationToken);
        if (note is null || note.UserId != userId)
        {
            return; // silently ignore
        }

        var ids = request.Items.Select(i => i.Id).ToHashSet();
        var sections = await _sectionReader.GetAllAsync(s => s.NoteId == request.NoteId && ids.Contains(s.Id), cancellationToken);

        foreach (var item in request.Items)
        {
            var section = sections.FirstOrDefault(s => s.Id == item.Id);
            if (section is null) continue;
            if (section.Type != SectionType.Timestamped) continue; // fixed sections not reorderable
            section.Order = item.Order;
            _sectionWriter.Update(section);
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}



using MediatR;
using AutoMapper;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Notes.GetNoteById;

public sealed class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, NoteDto>
{
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly IReadRepository<Section, int> _sectionReader;
    private readonly IReadRepository<Block, Guid> _blockReader;
    private readonly ISectionUiStateStore _uiStateStore;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetNoteByIdQueryHandler(IReadRepository<Note, int> noteReader,
        IReadRepository<Section, int> sectionReader,
        IReadRepository<Block, Guid> blockReader,
        ICurrentUserService currentUser,
        ISectionUiStateStore uiStateStore,
        IMapper mapper)
    {
        _noteReader = noteReader;
        _sectionReader = sectionReader;
        _blockReader = blockReader;
        _currentUser = currentUser;
        _uiStateStore = uiStateStore;
        _mapper = mapper;
    }

    public async Task<NoteDto> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await _noteReader.GetByIdAsync(request.Id, cancellationToken);
        if (note is null)
        {
            throw new NotFoundException($"Note with id {request.Id} not found.");
        }
        // Ownership check: only owner can access non-public notes
        var userId = _currentUser.UserId!; // ensured by IAuthenticatedRequest + rule
        if (!note.IsPublic && note.UserId != userId)
        {
            throw new NotFoundException($"Note with id {request.Id} not found.");
        }

        var dto = _mapper.Map<NoteDto>(note);
        dto.Sections.Clear();

        var sections = await _sectionReader.GetAllAsync(s => s.NoteId == note.Id, cancellationToken);
        var orderedSections = sections.OrderBy(s => s.Order).ToList();

        // Load all blocks for these sections in a single query to avoid N+1
        var sectionIds = orderedSections.Select(s => s.Id).ToList();
        var allBlocks = sectionIds.Count == 0
            ? new List<Block>()
            : await _blockReader.GetAllAsync(b => sectionIds.Contains(b.SectionId), cancellationToken);

        var blocksBySection = allBlocks
            .GroupBy(b => b.SectionId)
            .ToDictionary(g => g.Key, g => g.OrderBy(b => b.Order).ToList());

        // Load collapsed set for current user in one query
        var collapsedSet = await _uiStateStore.GetCollapsedSectionIdsAsync(userId, sectionIds, cancellationToken);

        foreach (var s in orderedSections)
        {
            var sDto = _mapper.Map<SectionDto>(s);
            sDto.Blocks.Clear();

            if (blocksBySection.TryGetValue(s.Id, out var blocks))
            {
                foreach (var b in blocks)
                {
                    sDto.Blocks.Add(_mapper.Map<BlockDto>(b));
                }
            }

            sDto.IsCollapsed = collapsedSet.Contains(s.Id);
            dto.Sections.Add(sDto);
        }

        return dto;
    }
}

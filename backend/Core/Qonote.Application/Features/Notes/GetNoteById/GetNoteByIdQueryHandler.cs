using MediatR;
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
    private readonly ICurrentUserService _currentUser;

    public GetNoteByIdQueryHandler(IReadRepository<Note, int> noteReader,
        IReadRepository<Section, int> sectionReader,
        IReadRepository<Block, Guid> blockReader,
        ICurrentUserService currentUser)
    {
        _noteReader = noteReader;
        _sectionReader = sectionReader;
        _blockReader = blockReader;
        _currentUser = currentUser;
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

        var dto = new NoteDto
        {
            Id = note.Id,
            CustomTitle = note.CustomTitle,
            YoutubeUrl = note.YoutubeUrl,
            VideoTitle = note.VideoTitle,
            ThumbnailUrl = note.ThumbnailUrl,
            ChannelName = note.ChannelName,
            VideoDuration = note.VideoDuration,
            IsPublic = note.IsPublic,
            PublicShareGuid = note.PublicShareGuid,
            Order = note.Order,
            Sections = new()
        };

        var sections = await _sectionReader.GetAllAsync(s => s.NoteId == note.Id, cancellationToken);
        sections = sections.OrderBy(s => s.Order).ToList();

        foreach (var s in sections)
        {
            var sDto = new SectionDto
            {
                Id = s.Id,
                Title = s.Title,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Order = s.Order,
                Type = s.Type,
                Blocks = new()
            };

            var blocks = await _blockReader.GetAllAsync(b => b.SectionId == s.Id, cancellationToken);
            foreach (var b in blocks.OrderBy(b => b.Order))
            {
                sDto.Blocks.Add(new BlockDto
                {
                    Id = b.Id,
                    Content = b.Content,
                    Type = b.Type,
                    Order = b.Order
                });
            }

            dto.Sections.Add(sDto);
        }

        return dto;
    }
}

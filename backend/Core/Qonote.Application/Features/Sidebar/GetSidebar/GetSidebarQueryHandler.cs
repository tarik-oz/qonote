using AutoMapper;
using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Sidebar.GetSidebar;

public sealed class GetSidebarQueryHandler : IRequestHandler<GetSidebarQuery, SidebarDto>
{
    private readonly IReadRepository<NoteGroup, int> _groupReader;
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly INoteQueries _noteQueries;

    public GetSidebarQueryHandler(
        IReadRepository<NoteGroup, int> groupReader,
        IReadRepository<Note, int> noteReader,
        ICurrentUserService currentUser,
        IMapper mapper,
        INoteQueries noteQueries)
    {
        _groupReader = groupReader;
        _noteReader = noteReader;
        _currentUser = currentUser;
        _mapper = mapper;
        _noteQueries = noteQueries;
    }

    public async Task<SidebarDto> Handle(GetSidebarQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule

        // Determine mode
        var groups = await _groupReader.GetAllAsync(g => g.UserId == userId, cancellationToken);
        if (groups.Count > 0)
        {
            // grouped mode
            var orderedGroups = groups
                .OrderByDescending(g => g.UpdatedAt ?? g.CreatedAt)
                .ThenBy(g => g.Order)
                .ToList();

            var groupDtos = new List<GroupItemDto>(orderedGroups.Count);
            foreach (var g in orderedGroups)
            {
                // NoteCount and preview thumbnails
                var orderedNotes = await _noteQueries.GetOrderedNotesForGroupAsync(g.Id, userId, offset: null, limit: null, cancellationToken);

                var thumbnails = orderedNotes
                    .Where(n => !string.IsNullOrWhiteSpace(n.ThumbnailUrl))
                    .Take(3)
                    .Select(n => n.ThumbnailUrl)
                    .ToList();

                var dto = _mapper.Map<GroupItemDto>(g);
                dto.Id = g.Id; // ensure explicit id set
                dto.NoteCount = orderedNotes.Count;
                dto.PreviewThumbnails = thumbnails;
                groupDtos.Add(dto);
            }

            return new SidebarDto
            {
                Mode = "grouped",
                Groups = groupDtos
            };
        }
        else
        {
            // flat mode
            var noteDtos = await _noteQueries.GetOrderedUngroupedNotesAsync(userId, request.Offset, request.Limit, cancellationToken);

            return new SidebarDto
            {
                Mode = "flat",
                Notes = noteDtos
            };
        }
    }
}

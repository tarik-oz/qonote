using AutoMapper;
using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Application.Abstractions.Caching;

namespace Qonote.Core.Application.Features.Sidebar.GetSidebar;

public sealed class GetSidebarQueryHandler : IRequestHandler<GetSidebarQuery, SidebarDto>
{
    private readonly IReadRepository<NoteGroup, int> _groupReader;
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly INoteQueries _noteQueries;
    private readonly ICacheService _cache;
    private readonly ICacheTtlProvider _ttl;

    public GetSidebarQueryHandler(
        IReadRepository<NoteGroup, int> groupReader,
        IReadRepository<Note, int> noteReader,
        ICurrentUserService currentUser,
        IMapper mapper,
        INoteQueries noteQueries,
        ICacheService cache,
        ICacheTtlProvider ttl)
    {
        _groupReader = groupReader;
        _noteReader = noteReader;
        _currentUser = currentUser;
        _mapper = mapper;
        _noteQueries = noteQueries;
        _cache = cache;
        _ttl = ttl;
    }

    public async Task<SidebarDto> Handle(GetSidebarQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule

        var key = $"qonote:sidebar:{userId}";

        var dto = await _cache.GetOrSetAsync(key, async ct =>
            {
                // Determine mode
                var groups = await _groupReader.GetAllAsync(g => g.UserId == userId, ct);
                if (groups.Count > 0)
                {
                    // grouped mode
                    var orderedGroups = groups
                        .OrderBy(g => g.Order)
                        .ToList();

                    var groupDtos = new List<GroupItemDto>(orderedGroups.Count);
                    foreach (var g in orderedGroups)
                    {
                        // NoteCount and preview thumbnails
                        var orderedNotes = await _noteQueries.GetOrderedNotesForGroupAsync(g.Id, userId, offset: null, limit: null, ct);

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
                    var noteDtos = await _noteQueries.GetOrderedUngroupedNotesAsync(userId, request.Offset, request.Limit, ct);

                    return new SidebarDto
                    {
                        Mode = "flat",
                        Notes = noteDtos
                    };
                }
            }, _ttl.GetSidebarTtl(), cancellationToken);
        return dto!;
    }
}

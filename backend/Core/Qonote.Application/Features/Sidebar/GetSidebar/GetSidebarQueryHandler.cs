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
    private readonly ICurrentUserService _currentUser;
    private readonly INoteQueries _noteQueries;
    private readonly ICacheService _cache;
    private readonly ICacheTtlProvider _ttl;

    public GetSidebarQueryHandler(
        IReadRepository<NoteGroup, int> groupReader,
        ICurrentUserService currentUser,
        INoteQueries noteQueries,
        ICacheService cache,
        ICacheTtlProvider ttl)
    {
        _groupReader = groupReader;
        _currentUser = currentUser;
        _noteQueries = noteQueries;
        _cache = cache;
        _ttl = ttl;
    }

    public async Task<SidebarDto> Handle(GetSidebarQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule

        // Cache key only by user; we will slice offset/limit in-memory after cache hit.
        var key = $"qonote:sidebar:{userId}";

        var cached = await _cache.GetOrSetAsync(key, async ct =>
        {
            // Build full, ordered sidebar state for the user
            var groups = await _groupReader.GetAllAsync(g => g.UserId == userId, ct);
            if (groups.Count > 0)
            {
                return await BuildGroupedSidebarAsync(groups, userId, ct);
            }
            else
            {
                return await BuildFlatSidebarAsync(userId, ct);
            }
        }, _ttl.GetSidebarTtl(), cancellationToken);

        // Apply offset/limit to the notes list if provided (only relevant for ungrouped list in the UI)
        if (cached is { Notes: { } notes } && (request.Offset is int || request.Limit is int))
        {
            var o = request.Offset.GetValueOrDefault();
            var l = request.Limit.GetValueOrDefault();
            var sliced = notes
                .Skip(o > 0 ? o : 0)
                .Take(l > 0 ? l : int.MaxValue)
                .ToList();

            return new SidebarDto(cached.Mode, cached.Groups, sliced);
        }

        return cached!;
    }

    private async Task<SidebarDto> BuildGroupedSidebarAsync(IList<NoteGroup> groups, string userId, CancellationToken ct)
    {
        var orderedGroups = groups
            .OrderBy(g => g.Order)
            .ThenBy(g => g.Id)
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

            var dto = new GroupItemDto(
                g.Id,
                g.Name,
                g.Order,
                orderedNotes.Count,
                thumbnails
            );
            groupDtos.Add(dto);
        }

        // Include all ungrouped notes; pagination will be applied in-memory based on the request
        var ungroupedNotes = await _noteQueries.GetOrderedUngroupedNotesAsync(userId, offset: null, limit: null, ct);

        return new SidebarDto(
            "grouped",
            groupDtos,
            ungroupedNotes
        );
    }

    private async Task<SidebarDto> BuildFlatSidebarAsync(string userId, CancellationToken ct)
    {
        var noteDtos = await _noteQueries.GetOrderedUngroupedNotesAsync(userId, offset: null, limit: null, ct);

        return new SidebarDto(
            "flat",
            null,
            noteDtos
        );
    }
}

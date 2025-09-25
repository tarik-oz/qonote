using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Core.Domain.Entities;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Infrastructure.Persistence.Queries;

public sealed class NoteQueries : INoteQueries
{
    private readonly ApplicationDbContext _db;
    private readonly IConfigurationProvider _mapperConfig;

    public NoteQueries(ApplicationDbContext db, IConfigurationProvider mapperConfig)
    {
        _db = db;
        _mapperConfig = mapperConfig;
    }

    public async Task<List<NoteListItemDto>> GetOrderedNotesForGroupAsync(int groupId, string userId, int? offset, int? limit, CancellationToken cancellationToken)
    {
        var query = _db.Set<Note>()
            .AsNoTracking()
            .Where(n => !n.IsDeleted && n.UserId == userId && n.NoteGroupId == groupId)
            .OrderByDescending(n => n.UpdatedAt ?? n.CreatedAt)
            .ThenBy(n => n.Order)
            .AsQueryable();

        if (offset is int o && o > 0)
        {
            query = query.Skip(o);
        }
        if (limit is int l && l > 0)
        {
            query = query.Take(l);
        }

        return await query
            .ProjectTo<NoteListItemDto>(_mapperConfig)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NoteListItemDto>> GetOrderedUngroupedNotesAsync(string userId, int? offset, int? limit, CancellationToken cancellationToken)
    {
        var query = _db.Set<Note>()
            .AsNoTracking()
            .Where(n => !n.IsDeleted && n.UserId == userId && n.NoteGroupId == null)
            .OrderByDescending(n => n.UpdatedAt ?? n.CreatedAt)
            .ThenBy(n => n.Order)
            .AsQueryable();

        if (offset is int o && o > 0)
        {
            query = query.Skip(o);
        }
        if (limit is int l && l > 0)
        {
            query = query.Take(l);
        }

        return await query
            .ProjectTo<NoteListItemDto>(_mapperConfig)
            .ToListAsync(cancellationToken);
    }
}

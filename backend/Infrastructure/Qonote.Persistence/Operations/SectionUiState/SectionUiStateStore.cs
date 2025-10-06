using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Domain.Entities;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Infrastructure.Persistence.Operations.SectionUiState;

public sealed class SectionUiStateStore : ISectionUiStateStore
{
    private readonly ApplicationDbContext _db;
    public SectionUiStateStore(ApplicationDbContext db) { _db = db; }

    public async Task SetCollapsedAsync(string userId, int sectionId, bool isCollapsed, CancellationToken cancellationToken)
    {
        if (isCollapsed)
        {
            var exists = await _db.Set<SectionUIState>().AnyAsync(s => s.UserId == userId && s.SectionId == sectionId, cancellationToken);
            if (!exists)
            {
                await _db.Set<SectionUIState>().AddAsync(new SectionUIState { UserId = userId, SectionId = sectionId }, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            var rows = _db.Set<SectionUIState>().Where(s => s.UserId == userId && s.SectionId == sectionId);
            _db.RemoveRange(rows);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SetCollapsedBatchAsync(string userId, int noteId, IEnumerable<(int SectionId, bool IsCollapsed)> items, CancellationToken cancellationToken)
    {
        var toCollapse = items.Where(i => i.IsCollapsed).Select(i => i.SectionId).ToHashSet();
        var toExpand = items.Where(i => !i.IsCollapsed).Select(i => i.SectionId).ToHashSet();

        if (toExpand.Count > 0)
        {
            var rows = _db.Set<SectionUIState>().Where(s => s.UserId == userId && toExpand.Contains(s.SectionId));
            _db.RemoveRange(rows);
        }

        if (toCollapse.Count > 0)
        {
            var existingCollapsed = await _db.Set<SectionUIState>()
                .AsNoTracking()
                .Where(s => s.UserId == userId && toCollapse.Contains(s.SectionId))
                .Select(s => s.SectionId)
                .ToListAsync(cancellationToken);

            var missing = toCollapse.Except(existingCollapsed).ToList();
            foreach (var sid in missing)
            {
                await _db.Set<SectionUIState>().AddAsync(new SectionUIState { UserId = userId, SectionId = sid }, cancellationToken);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<HashSet<int>> GetCollapsedSectionIdsAsync(string userId, IEnumerable<int> sectionIds, CancellationToken cancellationToken)
    {
        var ids = sectionIds.ToList();
        if (ids.Count == 0) return new HashSet<int>();
        var collapsed = await _db.Set<SectionUIState>()
            .AsNoTracking()
            .Where(s => s.UserId == userId && ids.Contains(s.SectionId))
            .Select(s => s.SectionId)
            .ToListAsync(cancellationToken);
        return collapsed.ToHashSet();
    }
}



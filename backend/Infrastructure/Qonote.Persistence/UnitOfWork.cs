using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Caching;
using Qonote.Core.Domain.Common;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheInvalidationService _cacheInvalidation;

    public UnitOfWork(ApplicationDbContext context, ICacheInvalidationService cacheInvalidation)
    {
        _context = context;
        _cacheInvalidation = cacheInvalidation;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Capture affected userIds before save
        var userIds = _context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged && e.Entity is ISidebarAffecting)
            .Select(e => ((ISidebarAffecting)e.Entity).UserId)
            .Distinct()
            .ToHashSet();

        var result = await _context.SaveChangesAsync(cancellationToken);

        // Best-effort: remove sidebar cache for affected users
        if (userIds.Count > 0)
        {
            await _cacheInvalidation.RemoveSidebarForAsync(userIds, cancellationToken);
        }

        return result;
    }
}

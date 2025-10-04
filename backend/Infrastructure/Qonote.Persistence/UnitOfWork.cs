using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Caching;
using Qonote.Core.Domain.Common;
using Qonote.Infrastructure.Persistence.Context;
using Qonote.Infrastructure.Persistence.ChangeTracking;

namespace Qonote.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheInvalidationService _cacheInvalidation;
    private readonly IEnumerable<ISidebarImpactEvaluator> _sidebarEvaluators;

    public UnitOfWork(
        ApplicationDbContext context,
        ICacheInvalidationService cacheInvalidation,
        IEnumerable<ISidebarImpactEvaluator> sidebarEvaluators)
    {
        _context = context;
        _cacheInvalidation = cacheInvalidation;
        _sidebarEvaluators = sidebarEvaluators;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect affected userIds via registered evaluators (sidebar)
        var userIds = _sidebarEvaluators
            .SelectMany(ev => ev.CollectAffectedUserIds(_context))
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

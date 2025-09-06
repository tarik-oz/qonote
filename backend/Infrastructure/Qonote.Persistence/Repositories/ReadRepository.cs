using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Domain.Common;
using Qonote.Infrastructure.Persistence.Context;
using System.Linq.Expressions;

namespace Qonote.Infrastructure.Persistence.Repositories;

public class ReadRepository<T, TKey> : IReadRepository<T, TKey>
    where T : class, IEntityBase<TKey>
{
    private readonly ApplicationDbContext _context;

    public ReadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _context.Set<T>().Where(e => !e.IsDeleted);

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>()
            .Where(e => !e.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }
}

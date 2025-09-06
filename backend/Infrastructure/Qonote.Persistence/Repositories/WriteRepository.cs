using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Domain.Common;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Infrastructure.Persistence.Repositories;

public class WriteRepository<T, TKey> : IWriteRepository<T, TKey>
    where T : class, IEntityBase<TKey>
{
    private readonly ApplicationDbContext _context;

    public WriteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        _context.Entry(entity).State = EntityState.Modified;
    }
}
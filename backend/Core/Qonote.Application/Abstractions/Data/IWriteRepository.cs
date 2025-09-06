using Qonote.Core.Domain.Common;

namespace Qonote.Core.Application.Abstractions.Data;

public interface IWriteRepository<T, in TKey> where T : class, IEntityBase<TKey>
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
}

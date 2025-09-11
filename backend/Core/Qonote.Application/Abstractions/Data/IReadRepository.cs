using System.Linq.Expressions;
using Qonote.Core.Domain.Common;

namespace Qonote.Core.Application.Abstractions.Data;

public interface IReadRepository<T, in TKey> where T : class, IEntityBase<TKey> where TKey : notnull
{
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
}

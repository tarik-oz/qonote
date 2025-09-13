using Qonote.Core.Application.Abstractions.Rules.Models;

namespace Qonote.Core.Application.Abstractions.Rules;

public interface IBusinessRule<TRequest> where TRequest : notnull
{
    Task<IEnumerable<RuleViolation>> CheckAsync(TRequest request, CancellationToken cancellationToken);
}

using Qonote.Core.Application.Abstractions.Rules.Models;

namespace Qonote.Core.Application.Abstractions.Rules;

public interface IBusinessRule<in TRequest> where TRequest : notnull
{
    int Order { get; }
    Task<IEnumerable<RuleViolation>> CheckAsync(TRequest request, CancellationToken cancellationToken);
}

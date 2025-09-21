using MediatR;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Exceptions;

namespace Qonote.Core.Application.Behaviors;

public class BusinessRulesBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IBusinessRule<TRequest>> _rules;

    public BusinessRulesBehavior(IEnumerable<IBusinessRule<TRequest>> rules) => _rules = rules;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_rules.Any())
        {
            return await next();
        }

        // Run rules sequentially based on their Order property
        foreach (var rule in _rules.OrderBy(r => r.Order))
        {
            var violations = await rule.CheckAsync(request, cancellationToken);
            if (violations.Any())
            {
                // If a rule fails, throw immediately and stop processing further rules.
                throw new BusinessRuleException(violations);
            }
        }

        return await next();
    }
}

using MediatR;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Exceptions;

namespace Qonote.Core.Application.Behaviors;

public class BusinessRulesBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IBusinessRule<TRequest>> _rules;

    public BusinessRulesBehavior(IEnumerable<IBusinessRule<TRequest>> rules) => _rules = rules;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_rules.Any())
        {
            var results = await Task.WhenAll(_rules.Select(r => r.CheckAsync(request, cancellationToken)));
            var violations = results.SelectMany(v => v).Where(v => v is not null).ToList();

            if (violations.Count != 0)
            {
                throw new BusinessRuleException(violations!);
            }
        }

        return await next();
    }
}

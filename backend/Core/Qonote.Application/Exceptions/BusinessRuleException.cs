using Qonote.Core.Application.Abstractions.Rules.Models;

namespace Qonote.Core.Application.Exceptions;

public class BusinessRuleException : BaseException
{
    public IDictionary<string, string[]> Errors { get; }

    public BusinessRuleException(IEnumerable<RuleViolation> violations)
        : base("One or more business rule violations occurred.")
    {
        Errors = violations
            .GroupBy(v => v.Key, v => v.Message)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}

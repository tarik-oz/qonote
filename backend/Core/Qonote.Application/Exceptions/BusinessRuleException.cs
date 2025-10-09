using Qonote.Core.Application.Abstractions.Rules.Models;

namespace Qonote.Core.Application.Exceptions;

public class BusinessRuleException : BaseException
{
    public IDictionary<string, string[]> Errors { get; }

    public BusinessRuleException(IEnumerable<RuleViolation> violations)
        : base("One or more business rule violations occurred.")
    {
        // Base errors keyed by property
        var grouped = violations
            .GroupBy(v => v.Key, v => v.Message)
            .ToDictionary(g => g.Key, g => g.ToArray());

        // Flatten metadata under a consistent, catchable key pattern: "{key}.metadata.{metaKey}"
        foreach (var v in violations)
        {
            if (v.Metadata is null) continue;
            foreach (var kv in v.Metadata)
            {
                var metaKey = $"{v.Key}.metadata.{kv.Key}";
                if (!grouped.TryGetValue(metaKey, out var arr))
                {
                    grouped[metaKey] = new[] { kv.Value };
                }
                else
                {
                    grouped[metaKey] = arr.Concat(new[] { kv.Value }).ToArray();
                }
            }
        }

        Errors = grouped;
    }
}

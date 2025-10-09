namespace Qonote.Core.Application.Abstractions.Rules.Models;

public sealed class RuleViolation
{
    public string Key { get; }
    public string Message { get; }
    public IReadOnlyDictionary<string, string>? Metadata { get; }

    public RuleViolation(string key, string message)
    {
        Key = key;
        Message = message;
    }

    public RuleViolation(string key, string message, IReadOnlyDictionary<string, string> metadata)
    {
        Key = key;
        Message = message;
        Metadata = metadata;
    }
}

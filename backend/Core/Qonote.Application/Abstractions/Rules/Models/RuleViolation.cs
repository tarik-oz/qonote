namespace Qonote.Core.Application.Abstractions.Rules.Models;

public sealed class RuleViolation
{
    public string Key { get; }
    public string Message { get; }

    public RuleViolation(string key, string message)
    {
        Key = key;
        Message = message;
    }
}

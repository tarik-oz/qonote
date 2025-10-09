using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FluentValidation;

namespace Qonote.Core.Application.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> TrimmedNotEmpty<T>(this IRuleBuilder<T, string> rule,
        string message)
    {
        return rule.Must(v => !string.IsNullOrWhiteSpace(v?.Trim()))
                   .WithMessage(message);
    }

    public static IRuleBuilderOptions<T, string> TrimmedMaxLength<T>(this IRuleBuilder<T, string> rule,
        int max, string message)
    {
        return rule.Must(v => (v?.Trim().Length ?? 0) <= max)
                   .WithMessage(message);
    }

    public static IRuleBuilderOptions<T, string> TrimmedMatches<T>(this IRuleBuilder<T, string> rule,
        string pattern, string message, RegexOptions options = RegexOptions.None)
    {
        return rule.Must(v =>
        {
            var s = v?.Trim();
            if (string.IsNullOrEmpty(s)) return false;
            return Regex.IsMatch(s, pattern, options);
        }).WithMessage(message);
    }

    public static IRuleBuilderOptions<T, string> TrimmedEmail<T>(this IRuleBuilder<T, string> rule,
        string message)
    {
        return rule.Must(v =>
        {
            var s = v?.Trim();
            if (string.IsNullOrWhiteSpace(s)) return false;
            return new EmailAddressAttribute().IsValid(s);
        }).WithMessage(message);
    }
}

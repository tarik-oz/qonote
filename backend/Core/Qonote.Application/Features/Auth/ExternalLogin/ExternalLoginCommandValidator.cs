using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Auth.ExternalLogin;

public sealed class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator()
    {
        RuleFor(x => x.Code)
            .TrimmedNotEmpty("Code is required.");

        RuleFor(x => x.Provider)
            .TrimmedNotEmpty("Provider is required.")
            .Must(p => string.Equals(p.Trim(), "google", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only Google provider is supported.");
    }
}

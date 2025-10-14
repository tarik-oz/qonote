using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.CreatePromoCode;

public sealed class CreatePromoCodeCommandValidator : AbstractValidator<CreatePromoCodeCommand>
{
    public CreatePromoCodeCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(64)
            .Matches("^[A-Za-z0-9-]+$")
            .WithMessage("Code may contain only alphanumeric characters and dashes.");

        RuleFor(x => x.PlanCode)
            .NotEmpty();

        RuleFor(x => x.DurationMonths)
            .GreaterThan(0);

        RuleFor(x => x.MaxRedemptions)
            .GreaterThan(0)
            .When(x => x.MaxRedemptions.HasValue);

        RuleFor(x => x.ExpiresAtUtc)
            .Must(dt => dt > DateTime.UtcNow)
            .When(x => x.ExpiresAtUtc.HasValue)
            .WithMessage("ExpiresAtUtc must be in the future.");
    }
}

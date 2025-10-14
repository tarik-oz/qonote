using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.UpdatePromoCode;

public sealed class UpdatePromoCodeCommandValidator : AbstractValidator<UpdatePromoCodeCommand>
{
    public UpdatePromoCodeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.DurationMonths).GreaterThan(0).When(x => x.DurationMonths.HasValue);
        RuleFor(x => x.MaxRedemptions).GreaterThan(0).When(x => x.MaxRedemptions.HasValue);
        RuleFor(x => x.PlanCode).NotEmpty().When(x => x.PlanCode is not null);
        RuleFor(x => x.ExpiresAtUtc)
            .Must(dt => dt > DateTime.UtcNow)
            .When(x => x.ExpiresAtUtc.HasValue)
            .WithMessage("ExpiresAtUtc must be in the future.");
    }
}

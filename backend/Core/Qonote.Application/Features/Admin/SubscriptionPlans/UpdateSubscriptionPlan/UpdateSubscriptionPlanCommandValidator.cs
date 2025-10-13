using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.UpdateSubscriptionPlan;

public sealed class UpdateSubscriptionPlanCommandValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
{
    public UpdateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.PlanCode).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.MaxNoteCount)
            .Must(v => v >= 0 || v == -1)
            .WithMessage("Use -1 for unlimited or a non-negative integer.");
        RuleFor(x => x.Description)
            .MaximumLength(2048)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
        RuleFor(x => x.MonthlyPrice)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.YearlyPrice)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(10);
        RuleFor(x => x.TrialDays)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.ExternalProductId)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.ExternalProductId));
        RuleFor(x => x.ExternalPriceIdMonthly)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.ExternalPriceIdMonthly));
        RuleFor(x => x.ExternalPriceIdYearly)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.ExternalPriceIdYearly));
    }
}

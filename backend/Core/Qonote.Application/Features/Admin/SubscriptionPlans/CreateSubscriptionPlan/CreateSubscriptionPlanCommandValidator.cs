using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.CreateSubscriptionPlan;

public sealed class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.PlanCode).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.MaxNoteCount)
            .Must(v => v >= 0 || v == -1)
            .WithMessage("Use -1 for unlimited or a non-negative integer.");
    }
}

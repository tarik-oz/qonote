using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.CreateSubscriptionPlan;

public sealed class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.PlanCode).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.MaxNoteCount).GreaterThanOrEqualTo(0);
    }
}

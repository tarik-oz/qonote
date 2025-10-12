using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.DeleteSubscriptionPlan;

public sealed class DeleteSubscriptionPlanCommandValidator : AbstractValidator<DeleteSubscriptionPlanCommand>
{
    public DeleteSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

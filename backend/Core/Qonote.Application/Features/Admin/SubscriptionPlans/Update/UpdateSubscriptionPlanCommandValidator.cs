using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.Update;

public sealed class UpdateSubscriptionPlanCommandValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
{
    public UpdateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.PlanCode).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.MaxNoteCount).GreaterThanOrEqualTo(0);
    }
}

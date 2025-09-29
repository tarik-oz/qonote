using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.CreateUserSubscription;

public sealed class CreateUserSubscriptionCommandValidator : AbstractValidator<CreateUserSubscriptionCommand>
{
    public CreateUserSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PlanCode).NotEmpty();
        RuleFor(x => x.StartDateUtc)
            .LessThan(x => x.EndDateUtc)
            .WithMessage("StartDateUtc must be before EndDateUtc");
        RuleFor(x => x.Currency)
            .MaximumLength(10)
            .When(x => x.Currency != null);
        RuleFor(x => x.BillingPeriod)
            .MaximumLength(20)
            .When(x => x.BillingPeriod != null);
        RuleFor(x => x.PriceAmount)
            .GreaterThanOrEqualTo(0).When(x => x.PriceAmount.HasValue);
    }
}

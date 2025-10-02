using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.CreateUserSubscription;

public sealed class CreateUserSubscriptionCommandValidator : AbstractValidator<CreateUserSubscriptionCommand>
{
    public CreateUserSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PlanCode).NotEmpty();
        RuleFor(x => x.StartDateUtc)
            .LessThan(x => x.EndDateUtc!.Value)
            .When(x => x.EndDateUtc.HasValue)
            .WithMessage("StartDateUtc must be before EndDateUtc");
        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(10);
        RuleFor(x => x.PriceAmount)
            .GreaterThanOrEqualTo(0);
        RuleFor(x => x.BillingInterval)
            .IsInEnum();
    }
}

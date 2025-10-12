using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.CancelUserSubscription;

public sealed class CancelUserSubscriptionCommandValidator : AbstractValidator<CancelUserSubscriptionCommand>
{
    public CancelUserSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SubscriptionId).GreaterThan(0);
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}

using FluentValidation;

namespace Qonote.Core.Application.Features.Subscriptions.CancelMySubscription;

public sealed class CancelMySubscriptionCommandValidator : AbstractValidator<CancelMySubscriptionCommand>
{
    public CancelMySubscriptionCommandValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500);
    }
}

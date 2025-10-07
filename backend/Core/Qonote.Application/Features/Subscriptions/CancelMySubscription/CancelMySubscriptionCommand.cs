using MediatR;

namespace Qonote.Core.Application.Features.Subscriptions.CancelMySubscription;

public sealed record CancelMySubscriptionCommand(
    bool CancelAtPeriodEnd,
    string? Reason
) : IRequest<Unit>;

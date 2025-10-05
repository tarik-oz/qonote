using MediatR;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.CancelUserSubscription;

public sealed record CancelUserSubscriptionCommand(
    string UserId,
    int SubscriptionId,
    bool CancelAtPeriodEnd,
    string? Reason
) : IRequest<Unit>;



using MediatR;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.CreateUserSubscription;

public sealed record CreateUserSubscriptionCommand(
    string UserId,
    string PlanCode,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    decimal? PriceAmount,
    string? Currency,
    string? BillingPeriod
) : IRequest<int>;

using MediatR;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Admin.UserSubscriptions.CreateUserSubscription;

public sealed record CreateUserSubscriptionCommand(
    string UserId,
    string PlanCode,
    DateTime StartDateUtc,
    DateTime? EndDateUtc,
    decimal PriceAmount,
    string Currency,
    BillingInterval BillingInterval
) : IRequest<int>;

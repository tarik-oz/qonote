using MediatR;
using Qonote.Core.Application.Features.Subscriptions._Shared;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.GetSubscriptionPlanById;

public sealed record GetSubscriptionPlanByIdQuery(
    int Id
) : IRequest<SubscriptionPlanDto?>;

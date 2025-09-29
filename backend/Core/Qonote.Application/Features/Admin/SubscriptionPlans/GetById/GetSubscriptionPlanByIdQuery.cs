using MediatR;
using Qonote.Core.Application.Features.Admin.SubscriptionPlans._Shared;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.GetById;

public sealed record GetSubscriptionPlanByIdQuery(
    int Id
) : IRequest<SubscriptionPlanDto?>;

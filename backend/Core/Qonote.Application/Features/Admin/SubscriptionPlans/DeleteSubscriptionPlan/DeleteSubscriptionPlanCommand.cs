using MediatR;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.DeleteSubscriptionPlan;

public sealed record DeleteSubscriptionPlanCommand(
    int Id
) : IRequest;

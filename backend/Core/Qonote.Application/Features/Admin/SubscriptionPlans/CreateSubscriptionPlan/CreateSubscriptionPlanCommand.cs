using MediatR;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.CreateSubscriptionPlan;

public sealed record CreateSubscriptionPlanCommand(
    string PlanCode,
    string Name,
    int MaxNoteCount
) : IRequest<int>;

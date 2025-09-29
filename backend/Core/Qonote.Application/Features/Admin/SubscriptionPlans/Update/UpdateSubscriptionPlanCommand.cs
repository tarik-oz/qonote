using MediatR;

namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans.Update;

public sealed record UpdateSubscriptionPlanCommand(
    int Id,
    string PlanCode,
    string Name,
    int MaxNoteCount
) : IRequest;

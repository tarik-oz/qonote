namespace Qonote.Core.Application.Features.Admin.SubscriptionPlans._Shared;

public sealed record SubscriptionPlanDto(
    int Id,
    string PlanCode,
    string Name,
    int MaxNoteCount
);

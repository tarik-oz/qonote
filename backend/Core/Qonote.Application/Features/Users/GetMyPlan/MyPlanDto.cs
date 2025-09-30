namespace Qonote.Core.Application.Features.Users.GetMyPlan;

public sealed record MyPlanDto(
    int PlanId,
    string PlanCode,
    string PlanName,
    int MaxNoteCount,
    DateTime? StartDateUtc,
    DateTime? EndDateUtc,
    int CurrentNoteCount,
    int RemainingNoteSlots
);

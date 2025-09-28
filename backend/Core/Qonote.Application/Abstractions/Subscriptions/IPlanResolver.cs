namespace Qonote.Core.Application.Abstractions.Subscriptions;

public interface IPlanResolver
{
    Task<EffectivePlan> GetEffectivePlanAsync(string userId, CancellationToken cancellationToken = default);
}

public sealed record EffectivePlan(
    int PlanId,
    string PlanCode,
    int MaxNoteCount
);

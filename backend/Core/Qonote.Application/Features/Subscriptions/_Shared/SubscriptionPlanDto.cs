namespace Qonote.Core.Application.Features.Subscriptions._Shared;

public record SubscriptionPlanDto
{
    public int Id { get; init; }
    public string PlanCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int MaxNoteCount { get; init; }
    public decimal MonthlyPrice { get; init; }
    public decimal YearlyPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public int TrialDays { get; init; }
    public bool IsActive { get; init; }
}


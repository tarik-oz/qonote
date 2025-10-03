using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Subscriptions._Shared;

public record UserSubscriptionDto
{
    public int Id { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public string PlanCode { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public SubscriptionStatus Status { get; init; }
    public bool AutoRenew { get; init; }
    public decimal PriceAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public BillingInterval BillingInterval { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public bool CancelAtPeriodEnd { get; init; }
    public DateTime? CurrentPeriodEnd { get; init; }
}


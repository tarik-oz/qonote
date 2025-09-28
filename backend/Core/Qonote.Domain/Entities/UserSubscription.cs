using Qonote.Core.Domain.Common;

namespace Qonote.Core.Domain.Entities;

public class UserSubscription : EntityBase<int>
{
    public string UserId { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public SubscriptionPlan? Plan { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Optional pricing snapshot (for history/reporting)
    public decimal? PriceAmount { get; set; }
    public string? Currency { get; set; } // e.g., USD, EUR, TL
    public string? BillingPeriod { get; set; } // e.g., Monthly, Yearly
}

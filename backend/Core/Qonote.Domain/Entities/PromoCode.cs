using Qonote.Core.Domain.Common;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Domain.Entities;

public class PromoCode : EntityBase<int>
{
    // Canonical code (stored uppercase); uniqueness enforced at DB level
    public string Code { get; set; } = string.Empty;

    // Target plan granted when redeemed
    public int PlanId { get; set; }
    public SubscriptionPlan? Plan { get; set; }

    // Duration in whole months applied to the subscription (StartDate.AddMonths(DurationMonths))
    public int DurationMonths { get; set; }

    // Redemption limits
    public int? MaxRedemptions { get; set; } // null = unlimited
    public int RedemptionCount { get; set; } = 0; // incremented on each successful redemption

    // Optional expiry (UTC). If null, no time limit.
    public DateTime? ExpiresAt { get; set; }

    // Business rules
    public bool SingleUsePerUser { get; set; } = true; // true => a user can redeem this code only once
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }

    // Audit / metadata
    public string? CreatedBy { get; set; } // Admin user ID who created the code (if tracked)
    public string? Description { get; set; }

    // Concurrency token (EF Core [Timestamp] configured via Fluent API)
    public byte[]? RowVersion { get; set; }

    // Navigation collection
    public ICollection<PromoCodeRedemption> Redemptions { get; set; } = new List<PromoCodeRedemption>();
}

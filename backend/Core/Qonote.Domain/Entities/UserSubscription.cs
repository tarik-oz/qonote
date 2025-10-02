using Qonote.Core.Domain.Common;
using Qonote.Core.Domain.Enums;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Domain.Entities;

public class UserSubscription : EntityBase<int>
{
    public string UserId { get; set; } = string.Empty;
    public int PlanId { get; set; }
    
    // Navigation Properties
    public ApplicationUser? User { get; set; }
    public SubscriptionPlan? Plan { get; set; }

    // Subscription Period
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; } // null = lifetime/free plan
    
    // Status
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    
    // Pricing Snapshot (ödeme anındaki fiyat)
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public BillingInterval BillingInterval { get; set; }
    
    // Auto-renewal
    public bool AutoRenew { get; set; } = true;
    
    // Trial
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    
    // Cancellation
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public bool CancelAtPeriodEnd { get; set; } = false; // true = cancel at period end
    
    // Payment Provider Integration
    public string? ExternalSubscriptionId { get; set; } // Provider subscription ID
    public string? ExternalCustomerId { get; set; } // Provider customer ID
    public string? ExternalPriceId { get; set; } // Provider price ID
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    
    // Payment Provider (extensible for any provider)
    public string PaymentProvider { get; set; } = "LemonSqueezy"; // "LemonSqueezy", "Stripe", "iyzico", "PayPal", "Manual"

    // Navigation Properties
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

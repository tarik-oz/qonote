using Qonote.Core.Domain.Common;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Domain.Entities;

public class SubscriptionPlan : EntityBase<int>
{
    public string PlanCode { get; set; } = string.Empty; // e.g., FREE, PREMIUM, PREMIUM_PLUS
    public string Name { get; set; } = string.Empty; // display name
    public string? Description { get; set; }
    
    // Features & Limits
    public int MaxNoteCount { get; set; }
    public bool IsActive { get; set; } = true; // Is the plan active?
    
    // Pricing
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string Currency { get; set; } = "USD"; // USD, TRY, EUR
    
    // Trial
    public int TrialDays { get; set; } = 0; // 0 = no trial
    
    // Payment Provider Integration
    public string? ExternalProductId { get; set; } // Provider Product ID
    public string? ExternalPriceIdMonthly { get; set; } // Provider Price ID (monthly)
    public string? ExternalPriceIdYearly { get; set; } // Provider Price ID (yearly)

    // Navigation Properties
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}

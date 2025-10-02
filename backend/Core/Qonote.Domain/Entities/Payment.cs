using Qonote.Core.Domain.Common;
using Qonote.Core.Domain.Enums;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Domain.Entities;

public class Payment : EntityBase<int>
{
    public string UserId { get; set; } = string.Empty;
    public int UserSubscriptionId { get; set; }
    
    // Navigation Properties
    public ApplicationUser? User { get; set; }
    public UserSubscription? UserSubscription { get; set; }
    
    // Payment Details
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    
    // Payment Provider Integration
    public string? ExternalPaymentId { get; set; } // Provider PaymentIntent/Transaction ID
    public string? ExternalChargeId { get; set; } // Provider Charge ID
    public string? ExternalInvoiceId { get; set; } // Provider Invoice ID
    
    // Payment Provider
    public string PaymentProvider { get; set; } = "LemonSqueezy";
    
    // Timestamps
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    
    // Failure
    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }
    
    // Refund
    public decimal? RefundedAmount { get; set; }
    public string? RefundReason { get; set; }
    
    // Metadata
    public string? Description { get; set; } // "Premium Plan - Monthly subscription"
    public string? InvoiceUrl { get; set; }
}



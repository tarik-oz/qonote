namespace Qonote.Core.Domain.Enums;

public enum SubscriptionStatus
{
    Trialing,         // Trial period is active
    Active,           // Subscription is active and paid
    PastDue,          // Payment failed, in retry period
    Cancelled,        // Cancelled but still active until period end
    Expired,          // Subscription has ended
    Incomplete,       // Initial payment not completed
    IncompleteExpired // Initial payment not completed before trial expired
}



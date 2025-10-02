namespace Qonote.Core.Domain.Enums;

public enum PaymentStatus
{
    Pending,          // Payment is waiting to be processed
    Processing,       // Payment is being processed
    Succeeded,        // Payment completed successfully
    Failed,           // Payment failed
    Cancelled,        // Payment was cancelled
    Refunded,         // Full refund issued
    PartiallyRefunded // Partial refund issued (e.g. $10 paid, $3 refunded)
}



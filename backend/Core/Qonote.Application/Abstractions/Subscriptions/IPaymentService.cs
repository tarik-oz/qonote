namespace Qonote.Core.Application.Abstractions.Subscriptions;

/// <summary>
/// Service for Lemon Squeezy API integration (3rd party communication only)
/// </summary>
public interface IPaymentService
{
    // Lemon Squeezy API - Checkout
    Task<string> CreateCheckoutUrlAsync(
        string email,
        string customerName,
        int variantId,
        string userId,
        int? priceId = null,
        string? successUrl = null,
        string? cancelUrl = null,
        CancellationToken cancellationToken = default);

    // Lemon Squeezy API - Subscription Management
    Task CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken = default);
    Task ResumeSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken = default);

    // Lemon Squeezy API - Customer
    Task<string?> GetOrCreateCustomerAsync(string email, string name, CancellationToken cancellationToken = default);

    // Webhook Handling
    Task HandleWebhookAsync(string payload, string? signature = null, CancellationToken cancellationToken = default);
}


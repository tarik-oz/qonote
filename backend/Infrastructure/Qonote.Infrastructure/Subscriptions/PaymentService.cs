using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Domain.Enums;

namespace Qonote.Infrastructure.Subscriptions;

/// <summary>
/// Lemon Squeezy API integration service
/// This service only handles HTTP communication with Lemon Squeezy
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly LemonSqueezySettings _settings;

    public PaymentService(HttpClient httpClient, Microsoft.Extensions.Options.IOptions<LemonSqueezySettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        
        // Configure HttpClient for Lemon Squeezy API
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
    }

    public Task<string> CreateCheckoutUrlAsync(string email, string customerName, int variantId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Lemon Squeezy checkout creation
        // POST /v1/checkouts
        // https://docs.lemonsqueezy.com/api/checkouts#create-a-checkout
        throw new NotImplementedException("Lemon Squeezy integration not yet implemented");
    }

    public Task CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Lemon Squeezy subscription cancellation
        // DELETE /v1/subscriptions/{id}
        // https://docs.lemonsqueezy.com/api/subscriptions#cancel-a-subscription
        throw new NotImplementedException("Lemon Squeezy integration not yet implemented");
    }

    public Task ResumeSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Lemon Squeezy subscription resume
        // PATCH /v1/subscriptions/{id}
        // https://docs.lemonsqueezy.com/api/subscriptions#update-a-subscription
        throw new NotImplementedException("Lemon Squeezy integration not yet implemented");
    }

    public Task<string?> GetOrCreateCustomerAsync(string email, string name, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Lemon Squeezy customer creation
        // POST /v1/customers
        // https://docs.lemonsqueezy.com/api/customers#create-a-customer
        throw new NotImplementedException("Lemon Squeezy integration not yet implemented");
    }

    public Task HandleWebhookAsync(string payload, string? signature = null, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Lemon Squeezy webhook handling
        // Verify signature, parse payload, process events
        // Events: order_created, subscription_created, subscription_updated, subscription_payment_success, subscription_cancelled
        // https://docs.lemonsqueezy.com/api/webhooks
        throw new NotImplementedException("Lemon Squeezy integration not yet implemented");
    }
}


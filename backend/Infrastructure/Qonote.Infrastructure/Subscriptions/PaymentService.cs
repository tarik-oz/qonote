using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;

namespace Qonote.Infrastructure.Infrastructure.Subscriptions;

/// <summary>
/// Lemon Squeezy API integration service
/// This service only handles HTTP communication with Lemon Squeezy
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly LemonSqueezySettings _settings;
    private readonly IReadRepository<SubscriptionPlan, int> _plans;
    private readonly IReadRepository<UserSubscription, int> _subsReader;
    private readonly IWriteRepository<UserSubscription, int> _subsWriter;
    private readonly IWriteRepository<Payment, int> _paymentsWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICacheInvalidationService _cacheInvalidation;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        HttpClient httpClient,
        Microsoft.Extensions.Options.IOptions<LemonSqueezySettings> settings,
        IReadRepository<SubscriptionPlan, int> plans,
        IReadRepository<UserSubscription, int> subsReader,
        IWriteRepository<UserSubscription, int> subsWriter,
        IWriteRepository<Payment, int> paymentsWriter,
        IUnitOfWork uow,
        ICacheInvalidationService cacheInvalidation,
        ILogger<PaymentService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _plans = plans;
        _subsReader = subsReader;
        _subsWriter = subsWriter;
        _paymentsWriter = paymentsWriter;
        _uow = uow;
        _cacheInvalidation = cacheInvalidation;
        _logger = logger;

        // Configure HttpClient for Lemon Squeezy API
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        _logger.LogDebug("LemonSqueezy HttpClient configured. baseUrl={BaseUrl}", _settings.BaseUrl);
    }

    public async Task<string> CreateCheckoutUrlAsync(string email, string customerName, int variantId, string userId, int? priceId = null, string? successUrl = null, string? cancelUrl = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("LS CreateCheckoutUrlAsync started. userId={UserId}, variantId={VariantId}", userId, variantId);
        if (string.IsNullOrWhiteSpace(_settings.StoreId))
            throw new InvalidOperationException("LemonSqueezy:StoreId is not configured.");

        if (!int.TryParse(_settings.StoreId, out var storeId))
            throw new InvalidOperationException("LemonSqueezy:StoreId must be a number.");

        var checkoutAttributes = new Dictionary<string, object?>
        {
            ["checkout_data"] = new
            {
                email = email,
                name = customerName,
                custom = new { user_id = userId }
            }
        };

        if (!string.IsNullOrWhiteSpace(successUrl))
        {
            checkoutAttributes["product_options"] = new
            {
                redirect_url = successUrl
            };
        }

        var payload = new
        {
            data = new
            {
                type = "checkouts",
                attributes = checkoutAttributes,
                relationships = new
                {
                    store = new
                    {
                        data = new
                        {
                            type = "stores",
                            id = storeId.ToString()
                        }
                    },
                    variant = new
                    {
                        data = new
                        {
                            type = "variants",
                            id = variantId.ToString()
                        }
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "checkouts");
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        request.Content = new StringContent(json, Encoding.UTF8, "application/vnd.api+json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("LS checkout create failed. userId={UserId}, variantId={VariantId}, status={StatusCode}", userId, variantId, (int)response.StatusCode);
            throw new InvalidOperationException($"Lemon Squeezy error ({(int)response.StatusCode})");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;
        var data = root.GetProperty("data");
        var responseAttributes = data.GetProperty("attributes");

        if (responseAttributes.TryGetProperty("url", out var urlProp) && urlProp.ValueKind == JsonValueKind.String)
        {
            var url = urlProp.GetString()!;
            _logger.LogInformation("LS checkout create succeeded. userId={UserId}, variantId={VariantId}", userId, variantId);
            return url;
        }
        if (responseAttributes.TryGetProperty("checkout_url", out var checkoutUrlProp) && checkoutUrlProp.ValueKind == JsonValueKind.String)
        {
            var url = checkoutUrlProp.GetString()!;
            _logger.LogInformation("LS checkout create succeeded. userId={UserId}, variantId={VariantId}", userId, variantId);
            return url;
        }

        throw new InvalidOperationException("Lemon Squeezy response did not include a checkout URL.");
    }

    public async Task CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("LS cancel subscription started. externalSubscriptionId={ExternalSubscriptionId}", externalSubscriptionId);
        var payload = new
        {
            data = new
            {
                type = "subscriptions",
                id = externalSubscriptionId,
                attributes = new { cancelled = true }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Patch, $"subscriptions/{externalSubscriptionId}");
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/vnd.api+json");
        using var resp = await _httpClient.SendAsync(req, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("LS cancel subscription failed. externalSubscriptionId={ExternalSubscriptionId}, status={StatusCode}", externalSubscriptionId, (int)resp.StatusCode);
        }
        resp.EnsureSuccessStatusCode();
        _logger.LogInformation("LS cancel subscription succeeded. externalSubscriptionId={ExternalSubscriptionId}", externalSubscriptionId);
    }

    public async Task ResumeSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("LS resume subscription started. externalSubscriptionId={ExternalSubscriptionId}", externalSubscriptionId);
        var payload = new
        {
            data = new
            {
                type = "subscriptions",
                id = externalSubscriptionId,
                attributes = new { cancelled = false }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Patch, $"subscriptions/{externalSubscriptionId}");
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/vnd.api+json");
        using var resp = await _httpClient.SendAsync(req, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("LS resume subscription failed. externalSubscriptionId={ExternalSubscriptionId}, status={StatusCode}", externalSubscriptionId, (int)resp.StatusCode);
        }
        resp.EnsureSuccessStatusCode();
        _logger.LogInformation("LS resume subscription succeeded. externalSubscriptionId={ExternalSubscriptionId}", externalSubscriptionId);
    }

    public async Task<string?> GetOrCreateCustomerAsync(string email, string name, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("LS getOrCreateCustomer started.");
        var payload = new
        {
            data = new
            {
                type = "customers",
                attributes = new { name = name, email = email }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "customers");
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/vnd.api+json");
        using var resp = await _httpClient.SendAsync(req, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("LS getOrCreateCustomer failed. status={StatusCode}", (int)resp.StatusCode);
            return null;
        }

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(cancellationToken));
        var id = doc.RootElement.GetProperty("data").GetProperty("id").GetString();
        _logger.LogInformation("LS getOrCreateCustomer succeeded. externalCustomerId={ExternalCustomerId}", id);
        return id;
    }

    public async Task HandleWebhookAsync(string payload, string? signature = null, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("n");
        using var corrScope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["Source"] = "LemonSqueezy"
        });
        _logger.LogDebug("Webhook received from LS. signaturePresent={HasSig}", !string.IsNullOrWhiteSpace(signature));
        // Verify signature (HMAC-SHA256, hex)
        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
            throw new InvalidOperationException("LemonSqueezy:WebhookSecret not configured.");

        if (!VerifySignature(_settings.WebhookSecret, payload, signature))
        {
            _logger.LogWarning("Invalid webhook signature.");
            throw new UnauthorizedAccessException("Invalid webhook signature.");
        }

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        var meta = root.TryGetProperty("meta", out var metaEl) ? metaEl : default;
        var eventName = meta.ValueKind != JsonValueKind.Undefined && meta.TryGetProperty("event_name", out var ev) ? ev.GetString() : null;
        var customData = meta.ValueKind != JsonValueKind.Undefined && meta.TryGetProperty("custom_data", out var cd) ? cd : default;
        var userId = customData.ValueKind != JsonValueKind.Undefined && customData.TryGetProperty("user_id", out var uid) ? uid.GetString() : null;

        var data = root.GetProperty("data");
        var dataId = data.GetProperty("id").GetString();
        var attributes = data.GetProperty("attributes");
        // Extract optional customer and price identifiers if available
        string? externalCustomerId = null;
        if (attributes.TryGetProperty("customer_id", out var custId))
        {
            externalCustomerId = custId.ValueKind == JsonValueKind.String ? custId.GetString() : custId.GetInt32().ToString();
        }

        using var eventScope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["LsEvent"] = eventName,
            ["LsDataId"] = dataId,
            ["HasUserId"] = !string.IsNullOrWhiteSpace(userId)
        });
        _logger.LogInformation("Webhook parsed. event={Event}, dataId={DataId}, hasUserId={HasUserId}", eventName, dataId, !string.IsNullOrWhiteSpace(userId));
        switch (eventName)
        {
            case "subscription_created":
            case "subscription_updated":
            case "subscription_cancelled":
                await HandleSubscriptionEvent(userId, dataId!, attributes, cancellationToken);
                break;
            case "subscription_payment_success":
                await HandleSubscriptionPaymentSuccess(userId, dataId, attributes, cancellationToken);
                break;
            default:
                _logger.LogDebug("Webhook event ignored. event={Event}", eventName);
                break;
        }
    }

    private static bool VerifySignature(string secret, string payload, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature)) return false;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expected = Convert.ToHexString(hash).ToLowerInvariant();
        return TimingSafeEquals(expected, signature.ToLowerInvariant());
    }

    private static bool TimingSafeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var result = 0;
        for (int i = 0; i < a.Length; i++) result |= a[i] ^ b[i];
        return result == 0;
    }

    private async Task HandleSubscriptionEvent(string? userId, string externalSubscriptionId, JsonElement attributes, CancellationToken ct)
    {
        _logger.LogDebug("HandleSubscriptionEvent. externalSubscriptionId={ExternalSubscriptionId}", externalSubscriptionId);
        int? variantId = null;
        if (attributes.TryGetProperty("variant_id", out var vid) && vid.TryGetInt32(out var v)) variantId = v;
        // Extract external customer id if present
        string? externalCustomerId = null;
        if (attributes.TryGetProperty("customer_id", out var custId))
        {
            externalCustomerId = custId.ValueKind == JsonValueKind.String ? custId.GetString() : custId.GetInt32().ToString();
        }
        string? externalPriceId = null;
        if (attributes.TryGetProperty("variant_id", out var variantEl))
        {
            externalPriceId = variantEl.ValueKind == JsonValueKind.String ? variantEl.GetString() : variantEl.GetInt32().ToString();
        }

        SubscriptionPlan? plan = null;
        if (variantId.HasValue)
        {
            var variantIdStr = variantId.Value.ToString();
            var withVariant = await _plans.GetAllAsync(
                p => p.ExternalPriceIdMonthly == variantIdStr || p.ExternalPriceIdYearly == variantIdStr,
                ct);
            plan = withVariant.FirstOrDefault();
            if (plan is not null)
            {
                _logger.LogDebug("Plan matched by variant. externalSubscriptionId={ExternalSubscriptionId}, planId={PlanId}", externalSubscriptionId, plan.Id);
            }
        }

        // Fallback: try finding by product_id if variant lookup failed
        if (plan is null && attributes.TryGetProperty("product_id", out var prodId) && prodId.TryGetInt32(out var productId))
        {
            var productIdStr = productId.ToString();
            var withProduct = await _plans.GetAllAsync(p => p.ExternalProductId == productIdStr, ct);
            plan = withProduct.FirstOrDefault();
            if (plan is not null)
            {
                _logger.LogDebug("Plan matched by product. externalSubscriptionId={ExternalSubscriptionId}, planId={PlanId}", externalSubscriptionId, plan.Id);
            }
        }
        if (plan is null)
        {
            _logger.LogWarning("No plan matched for subscription. externalSubscriptionId={ExternalSubscriptionId}", externalSubscriptionId);
        }

        // Map status
        var statusStr = attributes.TryGetProperty("status", out var st) ? st.GetString() : null;
        var status = statusStr switch
        {
            "active" => SubscriptionStatus.Active,
            "on_trial" or "trialing" => SubscriptionStatus.Trialing,
            "paused" => SubscriptionStatus.PastDue,
            "cancelled" => SubscriptionStatus.Cancelled,
            _ => SubscriptionStatus.Active
        };

        DateTime? currentPeriodEnd = null;
        if (attributes.TryGetProperty("renews_at", out var renewsAt) && renewsAt.ValueKind == JsonValueKind.String)
        {
            if (DateTime.TryParse(renewsAt.GetString(), out var d)) currentPeriodEnd = d.ToUniversalTime();
        }
        if (attributes.TryGetProperty("ends_at", out var endsAt) && endsAt.ValueKind == JsonValueKind.String)
        {
            if (DateTime.TryParse(endsAt.GetString(), out var e)) currentPeriodEnd = e.ToUniversalTime();
        }

        // Determine billing interval from attributes if available
        var interval = BillingInterval.Monthly;
        if (attributes.TryGetProperty("billing_interval", out var intervalProp) && intervalProp.ValueKind == JsonValueKind.String)
        {
            var intervalStr = intervalProp.GetString();
            interval = intervalStr?.Equals("year", StringComparison.OrdinalIgnoreCase) == true
                || intervalStr?.Equals("yearly", StringComparison.OrdinalIgnoreCase) == true
                ? BillingInterval.Yearly
                : BillingInterval.Monthly;
        }

        var subs = await _subsReader.GetAllAsync(s => s.ExternalSubscriptionId == externalSubscriptionId, ct);
        var sub = subs.FirstOrDefault();

        if (sub is null)
        {
            if (userId is null)
                throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("userId", "Webhook missing custom user_id.") });
            _logger.LogInformation("Creating new subscription from webhook. userId={UserId}, externalSubscriptionId={ExternalSubscriptionId}, status={Status}, interval={Interval}", userId, externalSubscriptionId, status, interval);
            sub = new UserSubscription
            {
                UserId = userId,
                PlanId = plan?.Id ?? 0,
                StartDate = DateTime.UtcNow,
                Status = status,
                BillingInterval = interval,
                AutoRenew = true,
                ExternalSubscriptionId = externalSubscriptionId,
                ExternalCustomerId = externalCustomerId,
                ExternalPriceId = externalPriceId,
                CurrentPeriodEnd = currentPeriodEnd,
                PaymentProvider = "LemonSqueezy"
            };
            await _subsWriter.AddAsync(sub, ct);
        }
        else
        {
            _logger.LogInformation("Updating subscription from webhook. externalSubscriptionId={ExternalSubscriptionId}, status={Status}, interval={Interval}", externalSubscriptionId, status, interval);
            if (plan is not null && plan.Id > 0) sub.PlanId = plan.Id;
            sub.Status = status;
            sub.CurrentPeriodEnd = currentPeriodEnd;
            sub.BillingInterval = interval;
            if (!string.IsNullOrWhiteSpace(externalCustomerId)) sub.ExternalCustomerId = externalCustomerId;
            if (!string.IsNullOrWhiteSpace(externalPriceId)) sub.ExternalPriceId = externalPriceId;
        }

        // If cancelled and an end date is provided, set EndDate; if resumed, clear EndDate
        if (status == SubscriptionStatus.Cancelled)
        {
            sub.EndDate = currentPeriodEnd;
        }
        else if (status == SubscriptionStatus.Active || status == SubscriptionStatus.Trialing)
        {
            sub.EndDate = null;
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogDebug("Subscription saved. externalSubscriptionId={ExternalSubscriptionId}, planId={PlanId}, status={Status}", externalSubscriptionId, sub.PlanId, sub.Status);

        // Invalidate user cache since plan has changed
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await _cacheInvalidation.RemoveMeAsync(userId, ct);
            _logger.LogDebug("Cache invalidated for user. userId={UserId}", userId);
        }
    }

    private async Task HandleSubscriptionPaymentSuccess(string? userId, string? invoiceId, JsonElement attributes, CancellationToken ct)
    {
        _logger.LogDebug("HandleSubscriptionPaymentSuccess started. userIdPresent={HasUserId}", !string.IsNullOrWhiteSpace(userId));
        decimal amount = 0m;
        if (attributes.TryGetProperty("total", out var total) && total.TryGetDecimal(out var totalDec))
        {
            amount = totalDec / 100; // Lemon Squeezy sends amount in cents
        }
        else if (attributes.TryGetProperty("subtotal", out var subtotal) && subtotal.TryGetDecimal(out var subtotalDec))
        {
            amount = subtotalDec / 100;
        }

        var currency = attributes.TryGetProperty("currency", out var curr) ? curr.GetString() ?? "USD" : "USD";

        string? invoiceUrl = null;
        if (attributes.TryGetProperty("urls", out var urls) && urls.TryGetProperty("invoice_url", out var invUrlProp))
        {
            invoiceUrl = invUrlProp.GetString();
        }

        string? subscriptionId = null;
        if (attributes.TryGetProperty("subscription_id", out var subId))
        {
            subscriptionId = subId.ValueKind == JsonValueKind.String ? subId.GetString() : subId.GetInt32().ToString();
        }

        if (userId is null)
        {
            _logger.LogDebug("Payment success ignored: missing userId.");
            return;
        }

        UserSubscription? sub = null;

        if (!string.IsNullOrWhiteSpace(subscriptionId))
        {
            var subsByExtId = await _subsReader.GetAllAsync(s => s.ExternalSubscriptionId == subscriptionId, ct);
            sub = subsByExtId.FirstOrDefault();
        }

        // Fallback: get latest subscription for user
        if (sub is null)
        {
            var subs = await _subsReader.GetAllAsync(s => s.UserId == userId, ct);
            sub = subs.OrderByDescending(s => s.StartDate).FirstOrDefault();
        }

        if (sub is null)
        {
            _logger.LogWarning("Payment success: no matching subscription found. userId={UserId}", userId);
            return;
        }

        var payment = new Payment
        {
            UserId = userId,
            UserSubscriptionId = sub.Id,
            Amount = amount,
            Currency = currency,
            Status = PaymentStatus.Succeeded,
            ExternalInvoiceId = invoiceId,
            InvoiceUrl = invoiceUrl,
            PaymentProvider = "LemonSqueezy",
            PaidAt = DateTime.UtcNow,
            Description = $"Subscription payment for plan {sub.PlanId}"
        };

        await _paymentsWriter.AddAsync(payment, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Payment recorded. userId={UserId}, subscriptionId={SubscriptionId}, amount={Amount} {Currency}", userId, sub.Id, amount, currency);
    }
}


namespace Qonote.Infrastructure.Infrastructure.Subscriptions;

public class LemonSqueezySettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.lemonsqueezy.com/v1";
}


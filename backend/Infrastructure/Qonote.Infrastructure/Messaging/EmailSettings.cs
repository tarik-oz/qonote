namespace Qonote.Infrastructure.Infrastructure.Messaging;

public class EmailSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
    // When true, emails will not be actually sent (useful for Development/testing)
    public bool DisableDelivery { get; set; } = false;
    // When true, email payloads will be logged via ILogger for debugging
    public bool LogEmails { get; set; } = false;
}

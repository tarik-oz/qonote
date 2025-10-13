using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qonote.Core.Application.Abstractions.Messaging;

namespace Qonote.Infrastructure.Infrastructure.Messaging;

public class AzureEmailService : IEmailService
{
    private readonly EmailClient? _emailClient;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<AzureEmailService> _logger;

    public AzureEmailService(IOptions<EmailSettings> emailSettings, ILogger<AzureEmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        if (!_emailSettings.DisableDelivery)
        {
            _emailClient = new EmailClient(_emailSettings.ConnectionString);
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        if (_emailSettings.LogEmails)
        {
            _logger.LogInformation("Email (subject: {Subject}) to {To}: {Preview}", subject, to, TrimForLog(body));
        }

        if (_emailSettings.DisableDelivery)
        {
            // Delivery disabled (e.g., Development). Skip sending.
            return;
        }

        if (_emailClient is null)
        {
            throw new InvalidOperationException("Email client is not configured.");
        }

        await _emailClient.SendAsync(
            wait: WaitUntil.Started, // Don't wait for completion, send in the background
            senderAddress: _emailSettings.SenderAddress,
            recipientAddress: to,
            subject: subject,
            htmlContent: body);
        _logger.LogInformation("Email queued for delivery. to={To}, subject={Subject}", to, subject);
    }

    private static string TrimForLog(string html, int max = 500)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        var text = html.Replace('\n', ' ').Replace("\r", " ");
        return text.Length <= max ? text : text.Substring(0, max) + "...";
    }
}

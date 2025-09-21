using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using Qonote.Core.Application.Abstractions.Messaging;

namespace Qonote.Infrastructure.Messaging;

public class AzureEmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly EmailSettings _emailSettings;

    public AzureEmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
        _emailClient = new EmailClient(_emailSettings.ConnectionString);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        await _emailClient.SendAsync(
            wait: WaitUntil.Started, // Don't wait for completion, send in the background
            senderAddress: _emailSettings.SenderAddress,
            recipientAddress: to,
            subject: subject,
            htmlContent: body);
    }
}

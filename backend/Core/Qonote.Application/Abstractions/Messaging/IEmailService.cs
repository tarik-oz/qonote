namespace Qonote.Core.Application.Abstractions.Messaging;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

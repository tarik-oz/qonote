namespace Qonote.Core.Application.Abstractions.Messaging;

public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Renders an email template by name, replacing placeholders with provided values.
    /// Placeholders are in the form of {{key}} in the template content.
    /// </summary>
    /// <param name="templateName">Template name without extension (e.g., "RegistrationConfirmation").</param>
    /// <param name="placeholders">Dictionary of placeholder values.</param>
    /// <returns>Rendered HTML string.</returns>
    Task<string> RenderAsync(string templateName, IDictionary<string, string> placeholders);
}
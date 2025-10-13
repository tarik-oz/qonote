using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Messaging;

namespace Qonote.Infrastructure.Infrastructure.Messaging;

public class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly ILogger<EmailTemplateRenderer> _logger;

    public EmailTemplateRenderer(ILogger<EmailTemplateRenderer> logger)
    {
        _logger = logger;
    }

    public async Task<string> RenderAsync(string templateName, IDictionary<string, string> placeholders)
    {
        var html = await ReadEmbeddedTemplateAsync(templateName + ".html");
        foreach (var kvp in placeholders)
        {
            html = html.Replace("{{" + kvp.Key + "}}", kvp.Value);
        }
        return html;
    }

    private async Task<string> ReadEmbeddedTemplateAsync(string fileName)
    {
        var asm = Assembly.GetExecutingAssembly();
        // Try to find by ending match to avoid hard-coding namespaces
        var resName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
        if (resName is null)
        {
            _logger.LogError("Email template resource '{FileName}' was not found. Available: {Resources}", fileName, string.Join(", ", asm.GetManifestResourceNames()));
            throw new FileNotFoundException($"Email template '{fileName}' not found as embedded resource.");
        }

        await using var stream = asm.GetManifestResourceStream(resName)!;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
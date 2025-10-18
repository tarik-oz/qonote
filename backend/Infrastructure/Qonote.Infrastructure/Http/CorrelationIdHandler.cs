using Microsoft.AspNetCore.Http;

namespace Qonote.Infrastructure.Infrastructure.Http;

/// <summary>
/// Propagates the X-Correlation-Id header on outgoing HTTP requests.
/// If none is present in the current request context, generates one.
/// </summary>
public class CorrelationIdHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string HeaderName = "X-Correlation-Id";

    public CorrelationIdHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        string? correlationId = null;

        if (httpContext != null)
        {
            // Prefer value stored in Items (set by request logging), else header, else create
            if (httpContext.Items.TryGetValue(HeaderName, out var obj) && obj is string s && !string.IsNullOrWhiteSpace(s))
            {
                correlationId = s;
            }
            else if (httpContext.Request.Headers.TryGetValue(HeaderName, out var values))
            {
                correlationId = values.FirstOrDefault();
            }
        }

        correlationId ??= Guid.NewGuid().ToString("n");

        if (!request.Headers.Contains(HeaderName))
        {
            request.Headers.Add(HeaderName, correlationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

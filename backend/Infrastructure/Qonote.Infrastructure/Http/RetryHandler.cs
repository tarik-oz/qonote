namespace Qonote.Infrastructure.Infrastructure.Http;

/// <summary>
/// Lightweight retry handler for transient HTTP errors (5xx, 408, timeouts).
/// </summary>
public class RetryHandler : DelegatingHandler
{
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;

    public RetryHandler(int maxRetries = 2, TimeSpan? baseDelay = null)
    {
        _maxRetries = Math.Max(0, maxRetries);
        _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(200);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        int attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                if (!IsTransient(response) || attempt > _maxRetries)
                {
                    return response;
                }
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Treat HttpClient timeout as transient
                if (attempt > _maxRetries) throw;
            }
            catch (HttpRequestException) when (attempt <= _maxRetries)
            {
                // Transient network error
            }

            var delay = TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
            try { await Task.Delay(delay, cancellationToken); } catch { /* ignore */ }
        }
    }

    private static bool IsTransient(HttpResponseMessage response)
    {
        var code = (int)response.StatusCode;
        return code == 408 || (code >= 500 && code < 600);
    }
}

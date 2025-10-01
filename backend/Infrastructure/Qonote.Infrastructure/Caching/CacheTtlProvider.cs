using Microsoft.Extensions.Options;
using Qonote.Core.Application.Abstractions.Caching;

namespace Qonote.Infrastructure.Infrastructure.Caching;

public sealed class CacheTtlProvider : ICacheTtlProvider
{
    private readonly CacheOptions _options;
    public CacheTtlProvider(IOptions<CacheOptions> options) => _options = options.Value;

    public TimeSpan GetMeTtl() => TimeSpan.FromSeconds(Math.Max(0, _options.Me.TtlSeconds));
    public TimeSpan GetSidebarTtl() => TimeSpan.FromSeconds(Math.Max(0, _options.Sidebar.TtlSeconds));
}

namespace Qonote.Core.Application.Abstractions.Caching;

public interface ICacheTtlProvider
{
    TimeSpan GetMeTtl();
    TimeSpan GetSidebarTtl();
}

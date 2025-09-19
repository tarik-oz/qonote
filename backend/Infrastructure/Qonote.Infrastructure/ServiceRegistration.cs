using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Infrastructure.Infrastructure.Security;
using Qonote.Infrastructure.Security;
using Qonote.Infrastructure.Security.External.Google;

namespace Qonote.Infrastructure.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TokenSettings>(configuration.GetSection("TokenSettings"));
        services.Configure<GoogleSettings>(configuration.GetSection("GoogleSettings"));

        services.AddTransient<ITokenService, TokenService>();
        services.AddTransient<IGoogleAuthService, GoogleAuthService>();

        services.AddHttpClient();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
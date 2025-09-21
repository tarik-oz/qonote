using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qonote.Core.Application.Abstractions.Authentication;
using Qonote.Core.Application.Abstractions.Messaging;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Storage;
using Qonote.Infrastructure.Infrastructure.Security;
using Qonote.Infrastructure.Messaging;
using Qonote.Infrastructure.Security;
using Qonote.Infrastructure.Security.External.Google;
using Qonote.Infrastructure.Storage;

namespace Qonote.Infrastructure.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TokenSettings>(configuration.GetSection("TokenSettings"));
        services.Configure<GoogleSettings>(configuration.GetSection("GoogleSettings"));
        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        services.AddTransient<ITokenService, TokenService>();
        services.AddTransient<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        services.AddTransient<IEmailService, AzureEmailService>();

        services.AddHttpClient();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Authentication;
using Qonote.Core.Application.Abstractions.Messaging;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Storage;
using Qonote.Infrastructure.Infrastructure.Security;
using Qonote.Infrastructure.Messaging;
using Qonote.Core.Domain.Identity;
using Qonote.Infrastructure.Persistence.Context;
using Qonote.Infrastructure.Security;
using Qonote.Infrastructure.Security.External.Google;
using Qonote.Infrastructure.Storage;
using Qonote.Infrastructure.YouTube;
using Qonote.Core.Application.Abstractions.YouTube;
using Qonote.Core.Application.Abstractions.Media;
using Qonote.Infrastructure.Media;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Infrastructure.Subscriptions;
using Qonote.Infrastructure.Infrastructure.Caching;
using Qonote.Core.Application.Abstractions.Caching;
using StackExchange.Redis;

namespace Qonote.Infrastructure.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TokenSettings>(configuration.GetSection("TokenSettings"));
        services.Configure<GoogleSettings>(configuration.GetSection("GoogleSettings"));
        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));
        services.Configure<YouTubeSettings>(configuration.GetSection("YouTube"));
        services.Configure<LemonSqueezySettings>(configuration.GetSection("LemonSqueezy"));
        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection("EmailSettings"))
            .Validate(e => e.DisableDelivery ||
                             (!string.IsNullOrWhiteSpace(e.ConnectionString) &&
                              !string.IsNullOrWhiteSpace(e.SenderAddress)),
                "EmailSettings require ConnectionString and SenderAddress when DisableDelivery is false.")
            .ValidateOnStart();

        services.AddTransient<ITokenService, TokenService>();
        services.AddTransient<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        services.AddTransient<IImageService, ImageService>();
        services.AddTransient<IEmailService, AzureEmailService>();
        services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddScoped<IPlanResolver, PlanResolver>();
        services.AddScoped<ILimitCheckerService, LimitCheckerService>();
        services.AddHttpClient<IPaymentService, PaymentService>(); // HttpClient for Lemon Squeezy API
        services.AddHttpClient<IYouTubeMetadataService, YouTubeMetadataService>();

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

        services.AddHttpClient();

        // Caching (Redis-only or NoOp)
        services.Configure<CacheOptions>(configuration.GetSection("Caching"));
        var cacheEnabled = configuration.GetValue<bool>("Caching:Enabled");
        var cacheConn = configuration["Caching:Redis:ConnectionString"];
        if (cacheEnabled && !string.IsNullOrWhiteSpace(cacheConn))
        {
            // Register ConnectionMultiplexer for advanced Redis operations (e.g., atomic INCR)
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(cacheConn));
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheConn;
            });
            services.AddSingleton<ICacheService, CacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, NoOpCacheService>();
        }
        services.AddSingleton<ICacheTtlProvider, CacheTtlProvider>();
        services.AddSingleton<ICacheInvalidationService, CacheInvalidationService>();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
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

namespace Qonote.Infrastructure.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TokenSettings>(configuration.GetSection("TokenSettings"));
        services.Configure<GoogleSettings>(configuration.GetSection("GoogleSettings"));
        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));
        services.Configure<YouTubeSettings>(configuration.GetSection("YouTube"));
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
        services.AddScoped<IPlanResolver, PlanResolver>();
        services.AddScoped<ILimitCheckerService, LimitCheckerService>();
        services.AddHttpClient<IYouTubeMetadataService, YouTubeMetadataService>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

        services.AddHttpClient();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
using System.Text;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Qonote.Core.Application;
using Qonote.Infrastructure.Infrastructure;
using Qonote.Infrastructure.Persistence;
using Qonote.Presentation.Api.Middleware;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Mvc;
using Qonote.Presentation.Api.Contracts;
using System.Text.Json.Serialization;
using Qonote.Presentation.Api.Infrastructure.Health;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(ms => ms.Value != null && ms.Value.Errors.Count > 0)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage)).ToArray()
                );

            var apiError = new ApiError(
                message: "Validation failed.",
                errors: errors,
                errorCode: "validation_failure"
            );

            return new BadRequestObjectResult(apiError);
        };
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Please enter token with Bearer prefix. Example: Bearer {token}"
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("db", tags: ["critical"])
    .AddCheck<RedisHealthCheck>("redis", tags: ["critical"])
    .AddCheck<BlobStorageHealthCheck>("blob", tags: ["critical"]);

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global limiter: per-user (100/min) if authenticated, else per-IP (60/min)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Skip rate limiting for health checks and webhooks
        var path = httpContext.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/health/", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/api/webhooks", StringComparison.OrdinalIgnoreCase))
        {
            return RateLimitPartition.GetNoLimiter("bypass");
        }

        // Determine key: user or IP
        var userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? httpContext.User?.FindFirst("sub")?.Value
                     ?? httpContext.User?.FindFirst("userid")?.Value;
        var isAuthenticated = !string.IsNullOrWhiteSpace(userId);
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (isAuthenticated)
        {
            // 100 tokens per minute per user
            return RateLimitPartition.GetTokenBucketLimiter($"user:{userId}", _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                TokensPerPeriod = 100,
                AutoReplenishment = true
            });
        }

        // Anonymous: 60 tokens per minute per IP
        return RateLimitPartition.GetTokenBucketLimiter($"ip:{clientIp}", _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 60,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 60,
            AutoReplenishment = true
        });
    });

    // Named policy: login (10/min per IP)
    options.AddPolicy("login", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter($"login:{clientIp}", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    // Named policy: register (3/hour per IP)
    options.AddPolicy("register", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter($"register:{clientIp}", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        var http = context.HttpContext;
        // Optional: set Retry-After header if provided by limiter
        if (context.Lease is not null && context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            http.Response.Headers["Retry-After"] = ((int)retryAfter.TotalSeconds).ToString();
        }

        // Log rejection
        var logger = http.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RateLimiter");
        logger.LogWarning("Rate limit exceeded. Path={Path}, User={UserId}, IP={IP}",
            http.Request.Path.Value,
            http.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anon",
            http.Connection.RemoteIpAddress?.ToString());

        http.Response.ContentType = "application/json";
        var apiError = new ApiError(
            message: "Too many requests. Please try again later.",
            errorCode: "rate_limited");
        await http.Response.WriteAsync(JsonSerializer.Serialize(apiError), token);
    };
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["TokenSettings:Issuer"],
            ValidAudience = builder.Configuration["TokenSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenSettings:Secret"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Request logging with Serilog (method, path, status, elapsed) and userId/clientIp enrichment
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diag, http) =>
    {
        var userId = http.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? http.User?.FindFirst("sub")?.Value
                     ?? http.User?.FindFirst("userid")?.Value
                     ?? http.User?.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            diag.Set("userId", userId);
        }

        var clientIp = http.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrWhiteSpace(clientIp))
        {
            diag.Set("clientIp", clientIp);
        }
    };
});

// Ensure default subscription plans are seeded (non-fatal if DB is unavailable)
try
{
    await Qonote.Infrastructure.Persistence.Seeding.SubscriptionPlanSeeding.EnsureDefaultPlansAsync(app.Services);
    await Qonote.Infrastructure.Persistence.Seeding.IdentitySeeding.EnsureAdminRoleAndBootstrapAsync(app.Services, app.Configuration);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Subscription plan seeding skipped (database not available). The app will continue to start.");
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Apply HTTPS redirect only for non-webhook paths
// Webhooks come from external services (like ngrok in dev or Lemon Squeezy in prod) and may use HTTP
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api/webhooks"),
    appBuilder => appBuilder.UseHttpsRedirection()
);

app.UseAuthentication();

// Apply Rate Limiting after auth so we can key by user
app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

// Health endpoints
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false, // always healthy if process is up
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { status = "Healthy" }));
    }
}).AllowAnonymous().DisableRateLimiting();

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    // Only run critical checks for readiness
    Predicate = check => check.Tags.Contains("critical"),
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString(), error = e.Value.Exception?.Message }),
            duration = report.TotalDuration.TotalMilliseconds
        };
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}).AllowAnonymous().DisableRateLimiting();

app.Run();

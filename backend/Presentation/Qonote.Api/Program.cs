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
    .AddCheck<DatabaseHealthCheck>("db");

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
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => true,
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
}).AllowAnonymous();

app.Run();

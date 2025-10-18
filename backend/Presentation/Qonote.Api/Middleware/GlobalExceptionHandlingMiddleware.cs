using System.Net;
using System.Text.Json;
using Qonote.Core.Application.Exceptions;
using Qonote.Presentation.Api.Contracts;

namespace Qonote.Presentation.Api.Middleware;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        ApiError apiError;
        var cid = context.Items["X-Correlation-Id"] as string
                  ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault();

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest; // 400
                apiError = new ApiError(validationEx.Message, validationEx.Errors, "validation_failure", cid);
                break;

            case BusinessRuleException ruleEx:
                statusCode = HttpStatusCode.UnprocessableEntity; // 422
                apiError = new ApiError(ruleEx.Message, ruleEx.Errors, "business_rule_violation", cid);
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound; // 404
                apiError = new ApiError(notFoundEx.Message, errorCode: "resource_not_found", correlationId: cid);
                break;

            case ConflictException conflictEx:
                statusCode = HttpStatusCode.Conflict; // 409
                apiError = new ApiError(conflictEx.Message, conflictEx.Errors, "resource_conflict", cid);
                break;

            case ExternalServiceException extEx:
                statusCode = HttpStatusCode.BadGateway; // 502
                var extras = new Dictionary<string, string[]>();
                extras["provider"] = new[] { extEx.Provider };
                if (extEx.StatusCode.HasValue)
                {
                    extras["upstreamStatus"] = new[] { extEx.StatusCode.Value.ToString() };
                }
                if (extEx.Errors is not null)
                {
                    foreach (var kvp in extEx.Errors)
                    {
                        extras[kvp.Key] = kvp.Value;
                    }
                }
                apiError = new ApiError(extEx.Message, extras, "external_service_error", cid);
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError; // 500
                apiError = new ApiError("An unexpected error occurred on the server.", errorCode: "internal_server_error", correlationId: cid);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(apiError);
        return context.Response.WriteAsync(jsonResponse);
    }
}

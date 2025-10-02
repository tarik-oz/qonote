using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Abstractions.Subscriptions;

namespace Qonote.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WebhooksController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(IPaymentService paymentService, ILogger<WebhooksController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Handle Lemon Squeezy webhook events
    /// This endpoint is called by Lemon Squeezy when payment/subscription events occur
    /// </summary>
    [HttpPost("lemonsqueezy")]
    public async Task<IActionResult> HandleLemonSqueezyWebhook(CancellationToken cancellationToken)
    {
        try
        {
            // Read raw body
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync(cancellationToken);

            // Get signature header for verification
            var signature = Request.Headers["X-Signature"].FirstOrDefault();

            _logger.LogInformation("Received Lemon Squeezy webhook");

            // PaymentService will verify signature and process events
            await _paymentService.HandleWebhookAsync(payload, signature, cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Lemon Squeezy webhook");
            return StatusCode(500);
        }
    }
}


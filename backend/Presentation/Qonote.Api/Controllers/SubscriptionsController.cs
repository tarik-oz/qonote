using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Enums;

namespace Qonote.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionsController : ControllerBase
{
    // TODO: Inject MediatR when implementing Command/Query handlers
    // private readonly IMediator _mediator;

    public SubscriptionsController()
    {
    }

    /// <summary>
    /// Get all available subscription plans
    /// </summary>
    [HttpGet("plans")]
    public async Task<ActionResult<List<SubscriptionPlanDto>>> GetPlans(CancellationToken cancellationToken)
    {
        // TODO: Implement with Query Handler
        // var result = await _mediator.Send(new GetSubscriptionPlansQuery(), cancellationToken);
        return Ok(new List<SubscriptionPlanDto>());
    }

    /// <summary>
    /// Create checkout session for a subscription plan
    /// </summary>
    [Authorize]
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutUrlDto>> CreateCheckout(
        [FromBody] CreateCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Implement with Command Handler
        // var result = await _mediator.Send(new CreateCheckoutCommand(request.PlanId, request.BillingInterval), cancellationToken);
        return Ok(new CheckoutUrlDto { CheckoutUrl = "" });
    }

    /// <summary>
    /// Get current user's active subscription
    /// </summary>
    [Authorize]
    [HttpGet("my-subscription")]
    public async Task<ActionResult<UserSubscriptionDto>> GetMySubscription(CancellationToken cancellationToken)
    {
        // TODO: Implement with Query Handler
        // var result = await _mediator.Send(new GetMySubscriptionQuery(), cancellationToken);
        return NotFound("No active subscription found");
    }

    /// <summary>
    /// Cancel current user's subscription
    /// </summary>
    [Authorize]
    [HttpPost("cancel")]
    public async Task<ActionResult> CancelSubscription(
        [FromBody] CancelSubscriptionRequest? request,
        CancellationToken cancellationToken)
    {
        // TODO: Implement with Command Handler
        // await _mediator.Send(new CancelSubscriptionCommand(request?.Reason), cancellationToken);
        return Ok(new { message = "Subscription cancelled successfully" });
    }

    /// <summary>
    /// Resume a cancelled subscription
    /// </summary>
    [Authorize]
    [HttpPost("resume")]
    public async Task<ActionResult> ResumeSubscription(CancellationToken cancellationToken)
    {
        // TODO: Implement with Command Handler
        // await _mediator.Send(new ResumeSubscriptionCommand(), cancellationToken);
        return Ok(new { message = "Subscription resumed successfully" });
    }

    /// <summary>
    /// Get current user's payment history
    /// </summary>
    [Authorize]
    [HttpGet("payments")]
    public async Task<ActionResult<List<PaymentDto>>> GetMyPayments(CancellationToken cancellationToken)
    {
        // TODO: Implement with Query Handler
        // var result = await _mediator.Send(new GetMyPaymentsQuery(), cancellationToken);
        return Ok(new List<PaymentDto>());
    }
}

// Request DTOs
public record CreateCheckoutRequest(int PlanId, BillingInterval BillingInterval);
public record CancelSubscriptionRequest(string? Reason);


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Application.Features.Subscriptions.ListPublicPlans;
using Qonote.Core.Application.Features.Subscriptions.CreateCheckout;
using Qonote.Core.Application.Features.Subscriptions.GetMySubscription;
using Qonote.Core.Application.Features.Subscriptions.CancelMySubscription;
using Qonote.Core.Application.Features.Subscriptions.ResumeMySubscription;
using Qonote.Core.Application.Features.Subscriptions.ListMyPayments;
using Qonote.Core.Domain.Enums;

namespace Qonote.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all available subscription plans
    /// </summary>
    [HttpGet("plans")]
    public async Task<ActionResult<List<SubscriptionPlanDto>>> GetPlans(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListPublicPlansQuery(), cancellationToken);
        return Ok(result);
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
        var result = await _mediator.Send(new CreateCheckoutCommand(request.PlanId, request.BillingInterval, request.SuccessUrl, request.CancelUrl), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get current user's active subscription
    /// </summary>
    [Authorize]
    [HttpGet("my-subscription")]
    public async Task<ActionResult<UserSubscriptionDto>> GetMySubscription(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMySubscriptionQuery(), cancellationToken);
        if (result is null) return NotFound("No active subscription found");
        return Ok(result);
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
        await _mediator.Send(new CancelMySubscriptionCommand(request?.CancelAtPeriodEnd ?? true, request?.Reason), cancellationToken);
        return Ok(new { message = "Subscription cancelled successfully" });
    }

    /// <summary>
    /// Resume a cancelled subscription
    /// </summary>
    [Authorize]
    [HttpPost("resume")]
    public async Task<ActionResult> ResumeSubscription(CancellationToken cancellationToken)
    {
        await _mediator.Send(new ResumeMySubscriptionCommand(), cancellationToken);
        return Ok(new { message = "Subscription resumed successfully" });
    }

    /// <summary>
    /// Get current user's payment history
    /// </summary>
    [Authorize]
    [HttpGet("payments")]
    public async Task<ActionResult<List<PaymentDto>>> GetMyPayments(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListMyPaymentsQuery(), cancellationToken);
        return Ok(result);
    }
}

// Request DTOs
public record CreateCheckoutRequest(int PlanId, BillingInterval BillingInterval, string? SuccessUrl, string? CancelUrl);
public record CancelSubscriptionRequest(bool CancelAtPeriodEnd, string? Reason);


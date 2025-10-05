using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Admin.UserSubscriptions.CreateUserSubscription;
using Qonote.Core.Application.Features.Admin.UserSubscriptions.ListUserSubscriptions;
using Qonote.Core.Application.Features.Admin.UserSubscriptions.CancelUserSubscription;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Enums;

namespace Qonote.Presentation.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("api/admin/users/{userId}/subscriptions")]
[ApiController]
public class UserSubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public UserSubscriptionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<UserSubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromRoute] string userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ListUserSubscriptionsQuery(userId), ct);
        return Ok(result);
    }

    public sealed record CreateUserSubscriptionBody(
        string PlanCode,
        DateTime StartDateUtc,
        DateTime? EndDateUtc,
        decimal PriceAmount,
        string Currency,
        BillingInterval BillingInterval
    );

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromRoute] string userId, [FromBody] CreateUserSubscriptionBody body, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateUserSubscriptionCommand(
            userId,
            body.PlanCode,
            body.StartDateUtc,
            body.EndDateUtc,
            body.PriceAmount,
            body.Currency,
            body.BillingInterval
        ), ct);

        return Ok(new { id });
    }

    public sealed record CancelUserSubscriptionBody(bool CancelAtPeriodEnd, string? Reason);

    [HttpDelete("{subscriptionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel([FromRoute] string userId, [FromRoute] int subscriptionId, [FromBody] CancelUserSubscriptionBody body, CancellationToken ct)
    {
        await _mediator.Send(new CancelUserSubscriptionCommand(userId, subscriptionId, body.CancelAtPeriodEnd, body.Reason), ct);
        return NoContent();
    }
}

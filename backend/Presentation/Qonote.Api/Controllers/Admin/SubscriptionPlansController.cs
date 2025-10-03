using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Application.Features.Admin.SubscriptionPlans.Create;
using Qonote.Core.Application.Features.Admin.SubscriptionPlans.Delete;
using Qonote.Core.Application.Features.Admin.SubscriptionPlans.GetById;
using Qonote.Core.Application.Features.Admin.SubscriptionPlans.List;
using Qonote.Core.Application.Features.Admin.SubscriptionPlans.Update;

namespace Qonote.Presentation.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("api/admin/subscription-plans")]
[ApiController]
public class SubscriptionPlansController : ControllerBase
{
    private readonly IMediator _mediator;
    public SubscriptionPlansController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _mediator.Send(new ListSubscriptionPlansQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubscriptionPlanByIdQuery(id), ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    public sealed record CreatePlanBody(string PlanCode, string Name, int MaxNoteCount);

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePlanBody body, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateSubscriptionPlanCommand(body.PlanCode, body.Name, body.MaxNoteCount), ct);
        return Ok(new { id });
    }

    public sealed record UpdatePlanBody(string PlanCode, string Name, int MaxNoteCount);

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlanBody body, CancellationToken ct)
    {
        await _mediator.Send(new UpdateSubscriptionPlanCommand(id, body.PlanCode, body.Name, body.MaxNoteCount), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSubscriptionPlanCommand(id), ct);
        return NoContent();
    }
}

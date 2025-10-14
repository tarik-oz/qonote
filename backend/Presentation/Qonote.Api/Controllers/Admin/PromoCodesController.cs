using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Admin.PromoCodes.CreatePromoCode;
using Qonote.Core.Application.Features.Admin.PromoCodes.UpdatePromoCode;
using Qonote.Core.Application.Features.Admin.PromoCodes.DeletePromoCode;
using Qonote.Core.Application.Features.Admin.PromoCodes.GetPromoCode;
using Qonote.Core.Application.Features.Admin.PromoCodes.ListPromoCodes;

namespace Qonote.Presentation.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("api/admin/promo-codes")]
[ApiController]
public class PromoCodesController : ControllerBase
{
    private readonly IMediator _mediator;
    public PromoCodesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool? isActive, [FromQuery] string? planCode, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var items = await _mediator.Send(new ListPromoCodesQuery(isActive, planCode, search, page, pageSize), ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetPromoCodeQuery(id), ct);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    public sealed record CreatePromoCodeBody(string Code, string PlanCode, int DurationMonths, int? MaxRedemptions, DateTime? ExpiresAtUtc, bool SingleUsePerUser, string? Description);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromoCodeBody body, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreatePromoCodeCommand(body.Code, body.PlanCode, body.DurationMonths, body.MaxRedemptions, body.ExpiresAtUtc, body.SingleUsePerUser, body.Description), ct);
        return Ok(new { id });
    }

    public sealed record UpdatePromoCodeBody(string? PlanCode, int? DurationMonths, int? MaxRedemptions, DateTime? ExpiresAtUtc, bool? SingleUsePerUser, bool? IsActive, string? Description);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePromoCodeBody body, CancellationToken ct)
    {
        await _mediator.Send(new UpdatePromoCodeCommand(id, body.PlanCode, body.DurationMonths, body.MaxRedemptions, body.ExpiresAtUtc, body.SingleUsePerUser, body.IsActive, body.Description), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeletePromoCodeCommand(id), ct);
        return NoContent();
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Sections.Create;
using Qonote.Core.Application.Features.Sections.Update;
using Qonote.Core.Application.Features.Sections.Delete;

namespace Qonote.Presentation.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public sealed class SectionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SectionsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateSectionCommand body, CancellationToken ct)
    {
        var id = await _mediator.Send(body, ct);
        return Ok(new { id });
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSectionCommand body, CancellationToken ct)
    {
        var cmd = body with { Id = id };
        await _mediator.Send(cmd, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSectionCommand(id), ct);
        return NoContent();
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Blocks.UpdateBlock;

namespace Qonote.Presentation.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public sealed class BlocksController : ControllerBase
{
    private readonly IMediator _mediator;
    public BlocksController(IMediator mediator) => _mediator = mediator;

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBlockCommand body, CancellationToken ct)
    {
        var cmd = body with { Id = id };
        await _mediator.Send(cmd, ct);
        return NoContent();
    }
}

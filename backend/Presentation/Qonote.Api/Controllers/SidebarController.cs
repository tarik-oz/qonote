using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;

namespace Qonote.Presentation.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SidebarController : ControllerBase
{
    private readonly IMediator _mediator;
    public SidebarController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(SidebarDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int? limit, [FromQuery] int? offset, CancellationToken ct)
    {
        var sidebar = await _mediator.Send(new GetSidebarQuery(limit, offset), ct);
        return Ok(sidebar);
    }
}

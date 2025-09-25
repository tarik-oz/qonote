using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.NoteGroups.CreateNoteGroup;
using Qonote.Core.Application.Features.NoteGroups.ListGroupNotes;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;

namespace Qonote.Presentation.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NoteGroupsController : ControllerBase
{
    private readonly IMediator _mediator;
    public NoteGroupsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateNoteGroupRequest body, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateNoteGroupCommand(body.Title), ct);
        return Ok(new { id });
    }

    [HttpGet("{id:int}/notes")]
    [ProducesResponseType(typeof(List<NoteListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListNotes(int id, [FromQuery] int? limit, [FromQuery] int? offset, CancellationToken ct)
    {
        var list = await _mediator.Send(new ListGroupNotesQuery(id, limit, offset), ct);
        return Ok(list);
    }
}

public sealed record CreateNoteGroupRequest(string Title);

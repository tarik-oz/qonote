using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.NoteGroups.CreateNoteGroup;
using Qonote.Core.Application.Features.NoteGroups.ListGroupNotes;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;
using Qonote.Core.Application.Features.NoteGroups.DeleteNoteGroup;
using Qonote.Core.Application.Features.NoteGroups.RenameNoteGroup;
using Qonote.Core.Application.Features.NoteGroups.ReorderNoteGroups;
using Qonote.Core.Application.Features.Notes.ReorderNotes;

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

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rename(int id, [FromBody] CreateNoteGroupRequest body, CancellationToken ct)
    {
        await _mediator.Send(new RenameNoteGroupCommand(id, body.Title), ct);
        return NoContent();
    }

    [HttpPatch("order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reorder([FromBody] List<NoteReorderItem> items, CancellationToken ct)
    {
        await _mediator.Send(new ReorderNoteGroupsCommand(items), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteNoteGroupCommand(id), ct);
        return NoContent();
    }
}

public sealed record CreateNoteGroupRequest(string Title);

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Notes.CreateNoteFromYoutubeUrl;
using Qonote.Core.Application.Features.Notes.GetNoteById;
using Qonote.Core.Application.Features.Notes.AssignGroup;
using Qonote.Core.Application.Features.Notes.ListFlatNotes;
using Qonote.Core.Application.Features.Notes.ReorderNotes;
using Qonote.Core.Application.Features.Notes.UpdateNote;
using Qonote.Core.Application.Features.Notes.SearchNotes;
using Qonote.Core.Application.Features.Sections.SetSectionUiStateBatch;

namespace Qonote.Presentation.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;
    public NotesController(IMediator mediator) => _mediator = mediator;

    [HttpPost("from-youtube")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFromYouTube([FromBody] CreateNoteFromYoutubeUrlCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return Ok(new { id });
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var note = await _mediator.Send(new GetNoteByIdQuery { Id = id }, ct);
        return Ok(note);
    }

    [HttpPatch("{noteId:int}/group/{groupId:int?}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignGroup(int noteId, int? groupId, CancellationToken ct)
    {
        await _mediator.Send(new AssignNoteGroupCommand(noteId, groupId), ct);
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Core.Application.Features.Sidebar.GetSidebar.NoteListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int? limit, [FromQuery] int? offset, CancellationToken ct)
    {
        var list = await _mediator.Send(new ListFlatNotesQuery(limit, offset), ct);
        return Ok(list);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchNotesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SearchNotesQuery
        {
            Query = q ?? string.Empty,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, ct);
        return Ok(result);
    }

    [HttpPatch("order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reorder([FromBody] List<NoteReorderItem> items, CancellationToken ct)
    {
        await _mediator.Send(new ReorderNotesCommand(items), ct);
        return NoContent();
    }

    [HttpPatch("groups/{groupId:int}/order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderInGroup(int groupId, [FromBody] List<NoteReorderItem> items, CancellationToken ct)
    {
        await _mediator.Send(new ReorderNotesCommand(items, groupId), ct);
        return NoContent();
    }

    [HttpPut("{noteId:int}/sections/ui-state-batch")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetSectionsUiStateBatch(int noteId, [FromBody] SetSectionUiStateBatchCommand body, CancellationToken ct)
    {
        var cmd = body with { NoteId = noteId };
        await _mediator.Send(cmd, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNoteCommand body, CancellationToken ct)
    {
        // Ensure route id is used
        var cmd = body with { Id = id };
        await _mediator.Send(cmd, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new Core.Application.Features.Notes.DeleteNote.DeleteNoteCommand(id), ct);
        return NoContent();
    }
}

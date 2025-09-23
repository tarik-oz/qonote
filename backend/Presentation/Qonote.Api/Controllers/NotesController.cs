using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Notes.CreateNoteFromYoutubeUrl;
using Qonote.Core.Application.Features.Notes.GetNoteById;

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
    public async Task<IActionResult> CreateFromYouTube([FromBody] CreateNoteFromYoutubeUrlCommand command)
    {
        var id = await _mediator.Send(command);
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
}

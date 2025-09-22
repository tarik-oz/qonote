using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Users.UpdateProfileInfo;
using Qonote.Core.Application.Features.Users.UpdateProfilePicture;
using Qonote.Core.Application.Features.Users.ChangePassword;

namespace Qonote.Presentation.Api.Controllers;

[Authorize]
[Route("api/me")]
[ApiController]
public class MeController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPatch("profile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfileInfo(UpdateProfileInfoCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("profile-picture")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureCommand command)
    {
        var newImageUrl = await _mediator.Send(command);
        return Ok(new { ProfilePictureUrl = newImageUrl });
    }

    [HttpPost("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}

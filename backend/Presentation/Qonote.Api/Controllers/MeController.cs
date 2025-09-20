using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Users.UpdateProfileInfo;
using Qonote.Core.Application.Features.Users.UpdateProfilePicture;

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

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfileInfo(UpdateProfileInfoCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("picture")]
    public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureCommand command)
    {
        var newImageUrl = await _mediator.Send(command);
        return Ok(new { ProfilePictureUrl = newImageUrl });
    }
}

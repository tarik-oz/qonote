using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Users.UpdateProfileInfo;
using Qonote.Core.Application.Features.Users.UpdateProfilePicture;
using Qonote.Core.Application.Features.Users.ChangePassword;
using Qonote.Core.Application.Features.Users.GetMyPlan;
using Qonote.Core.Application.Features.Users.GetMe;

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
    public async Task<IActionResult> UpdateProfileInfo(UpdateProfileInfoCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPut("profile-picture")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureCommand command, CancellationToken ct)
    {
        var newImageUrl = await _mediator.Send(command, ct);
        return Ok(new { ProfilePictureUrl = newImageUrl });
    }

    [HttpPost("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetMeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetMeQuery(), ct);
        return Ok(dto);
    }

    [HttpGet("plan")]
    [ProducesResponseType(typeof(MyPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPlan(CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetMyPlanQuery(), ct);
        return Ok(dto);
    }
}

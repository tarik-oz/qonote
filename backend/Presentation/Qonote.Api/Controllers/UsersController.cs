using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Users.UpdateProfileInfo;

namespace Qonote.Presentation.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfileInfo(UpdateProfileInfoCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}

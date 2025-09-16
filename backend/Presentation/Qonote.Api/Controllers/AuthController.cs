using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Auth.Register;
using Qonote.Core.Application.Features.Auth.Login;
using Qonote.Core.Application.Features.Auth.RefreshToken;
using Qonote.Core.Application.Features.Auth.Logout;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Qonote.Presentation.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        await _mediator.Send(new LogoutCommand { UserId = userId });
        return NoContent();
    }
}

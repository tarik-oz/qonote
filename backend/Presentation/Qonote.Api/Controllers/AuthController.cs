using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Auth.ExternalLogin;
using Qonote.Core.Application.Features.Auth.GoogleLoginUrl;
using Qonote.Core.Application.Features.Auth.Login;
using Qonote.Core.Application.Features.Auth.Logout;
using Qonote.Core.Application.Features.Auth.RefreshToken;
using Qonote.Core.Application.Features.Auth.Register;

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
        await _mediator.Send(new LogoutCommand());
        return NoContent();
    }

    [HttpGet("google-login-url")]
    public async Task<IActionResult> GetGoogleLoginUrl([FromQuery] GoogleLoginUrlQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("external-login")]
    public async Task<IActionResult> ExternalLogin(ExternalLoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qonote.Core.Application.Features.Auth.ConfirmEmail;
using Qonote.Core.Application.Features.Auth.ExternalLogin;
using Qonote.Core.Application.Features.Auth.GoogleLoginUrl;
using Qonote.Core.Application.Features.Auth.Login;
using Qonote.Core.Application.Features.Auth.Logout;
using Qonote.Core.Application.Features.Auth.RefreshToken;
using Qonote.Core.Application.Features.Auth.Register;
using Qonote.Core.Application.Features.Auth.SendConfirmationEmail;
using Qonote.Core.Application.Features.Auth.ForgotPassword;
using Qonote.Core.Application.Features.Auth.ResetPassword;

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

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterUserCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("email-confirmations")]
    [ProducesResponseType(typeof(SendConfirmationEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendConfirmationEmail(SendConfirmationEmailCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(), ct);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("google-login-url")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGoogleLoginUrl([FromQuery] GoogleLoginUrlQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("external-login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExternalLogin(ExternalLoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        // Always return 200 to avoid account enumeration
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok();
    }
}

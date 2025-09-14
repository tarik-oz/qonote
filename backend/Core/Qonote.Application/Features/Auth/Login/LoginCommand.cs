using MediatR;
using Qonote.Core.Application.Features.Auth._Shared;

namespace Qonote.Core.Application.Features.Auth.Login;

public sealed class LoginCommand : IRequest<LoginResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

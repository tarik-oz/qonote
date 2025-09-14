using MediatR;

namespace Qonote.Core.Application.Features.Auth.Logout;

public sealed class LogoutCommand : IRequest<Unit>
{
    public string UserId { get; set; } = string.Empty;
}

using MediatR;

namespace Qonote.Core.Application.Features.Auth.Logout;

public sealed class LogoutCommand : IRequest<Unit>
{
    // This command is parameterless because the user ID will be retrieved from the authenticated user's claims.
}


using MediatR;
using Qonote.Core.Application.Features.Auth._Shared;

namespace Qonote.Core.Application.Features.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponseDto>;

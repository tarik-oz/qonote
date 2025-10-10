using MediatR;
using Qonote.Core.Application.Features.Auth._Shared;

namespace Qonote.Core.Application.Features.Auth.ExternalLogin;

public sealed record ExternalLoginCommand(
    string Code,
    string Provider
) : IRequest<LoginResponseDto>;

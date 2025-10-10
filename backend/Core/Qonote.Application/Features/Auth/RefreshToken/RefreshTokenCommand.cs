using MediatR;
using Qonote.Core.Application.Features.Auth._Shared;

namespace Qonote.Core.Application.Features.Auth.RefreshToken;

public sealed record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken
) : IRequest<LoginResponseDto>;

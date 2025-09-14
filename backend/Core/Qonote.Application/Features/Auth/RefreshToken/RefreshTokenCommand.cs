using MediatR;
using Qonote.Core.Application.Features.Auth._Shared;

namespace Qonote.Core.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommand : IRequest<LoginResponseDto>
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

using MediatR;
using Qonote.Core.Application.Features.Auth._Shared;

namespace Qonote.Core.Application.Features.Auth.ExternalLogin;

public sealed class ExternalLoginCommand : IRequest<LoginResponseDto>
{
    public string Code { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty; // e.g., "google", "facebook"
}

using MediatR;

namespace Qonote.Core.Application.Features.Auth.GoogleLoginUrl;

public sealed class GoogleLoginUrlQuery : IRequest<GoogleLoginUrlResponseDto>
{
    public string RedirectUri { get; set; } = string.Empty;
}

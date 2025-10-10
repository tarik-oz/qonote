using MediatR;

namespace Qonote.Core.Application.Features.Auth.GoogleLoginUrl;

public sealed record GoogleLoginUrlQuery(
    string? RedirectUri
) : IRequest<GoogleLoginUrlResponseDto>;

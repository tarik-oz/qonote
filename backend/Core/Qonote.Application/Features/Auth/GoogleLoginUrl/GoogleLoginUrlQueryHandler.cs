using MediatR;
using Qonote.Core.Application.Abstractions.Authentication;

namespace Qonote.Core.Application.Features.Auth.GoogleLoginUrl;

public sealed class GoogleLoginUrlQueryHandler : IRequestHandler<GoogleLoginUrlQuery, GoogleLoginUrlResponseDto>
{
    private readonly IGoogleAuthService _googleAuthService;

    public GoogleLoginUrlQueryHandler(IGoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
    }

    public Task<GoogleLoginUrlResponseDto> Handle(GoogleLoginUrlQuery request, CancellationToken cancellationToken)
    {
        var redirect = string.IsNullOrWhiteSpace(request.RedirectUri) ? string.Empty : request.RedirectUri!.Trim();
        var url = _googleAuthService.GenerateAuthUrl(redirect);
        return Task.FromResult(new GoogleLoginUrlResponseDto(url));
    }
}

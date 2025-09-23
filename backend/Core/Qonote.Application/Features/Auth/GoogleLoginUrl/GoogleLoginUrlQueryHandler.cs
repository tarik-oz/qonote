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
        // Always use configured RedirectUri; passing empty triggers fallback in the service
        var url = _googleAuthService.GenerateAuthUrl(string.Empty);

        var response = new GoogleLoginUrlResponseDto
        {
            GoogleAuthUrl = url
        };

        return Task.FromResult(response);
    }
}

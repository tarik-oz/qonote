using MediatR;
using Qonote.Core.Application.Abstractions.Security;

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
        var url = _googleAuthService.GenerateAuthUrl(request.RedirectUri);

        var response = new GoogleLoginUrlResponseDto
        {
            GoogleAuthUrl = url
        };

        return Task.FromResult(response);
    }
}

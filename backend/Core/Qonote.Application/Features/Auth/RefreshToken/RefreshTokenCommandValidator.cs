using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .TrimmedNotEmpty("Access token is required.");

        RuleFor(x => x.RefreshToken)
            .TrimmedNotEmpty("Refresh token is required.");
    }
}

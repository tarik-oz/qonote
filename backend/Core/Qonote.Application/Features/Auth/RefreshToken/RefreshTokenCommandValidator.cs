using FluentValidation;

namespace Qonote.Core.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

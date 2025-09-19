using FluentValidation;

namespace Qonote.Core.Application.Features.Auth.GoogleLoginUrl;

public sealed class GoogleLoginUrlQueryValidator : AbstractValidator<GoogleLoginUrlQuery>
{
    public GoogleLoginUrlQueryValidator()
    {
        RuleFor(x => x.RedirectUri).NotEmpty();
    }
}

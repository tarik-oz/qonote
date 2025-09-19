using FluentValidation;

namespace Qonote.Core.Application.Features.Auth.ExternalLogin;

public sealed class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Provider).NotEmpty();
    }
}

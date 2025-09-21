using FluentValidation;

namespace Qonote.Core.Application.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .WithMessage("Confirmation code must be 6 characters long.");
    }
}

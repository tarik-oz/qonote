using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .TrimmedNotEmpty("Email is required.")
            .TrimmedEmail("A valid email address is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Confirmation code is required.")
            .Length(6).WithMessage("Confirmation code must be 6 characters long.")
            .Matches("^[0-9]{6}$").WithMessage("Confirmation code must contain digits only.");
    }
}

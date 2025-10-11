using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Auth.Register;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .TrimmedNotEmpty("Name is required.")
            .TrimmedMaxLength(50, "Name must not exceed 50 characters.")
            .TrimmedMatches(@"^[\p{L}' ]+$", "Name can only contain letters (including Turkish), spaces, and apostrophes.");

        RuleFor(x => x.Surname)
            .TrimmedNotEmpty("Surname is required.")
            .TrimmedMaxLength(50, "Surname must not exceed 50 characters.")
            .TrimmedMatches(@"^[\p{L}' ]+$", "Surname can only contain letters (including Turkish), spaces, and apostrophes.");

        RuleFor(x => x.Email)
            .TrimmedNotEmpty("Email is required.")
            .TrimmedEmail("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}

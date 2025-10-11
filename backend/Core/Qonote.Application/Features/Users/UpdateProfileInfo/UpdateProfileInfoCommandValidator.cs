using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Users.UpdateProfileInfo;

public sealed class UpdateProfileInfoCommandValidator : AbstractValidator<UpdateProfileInfoCommand>
{
    public UpdateProfileInfoCommandValidator()
    {
        RuleFor(x => x.Name)
            .TrimmedNotEmpty("Name is required.")
            .TrimmedMaxLength(50, "Name must not exceed 50 characters.")
            .TrimmedMatches(@"^[\p{L}' ]+$", "Name can only contain letters (including Turkish), spaces, and apostrophes.");

        RuleFor(x => x.Surname)
            .TrimmedNotEmpty("Surname is required.")
            .TrimmedMaxLength(50, "Surname must not exceed 50 characters.")
            .TrimmedMatches(@"^[\p{L}' ]+$", "Surname can only contain letters (including Turkish), spaces, and apostrophes.");
    }
}

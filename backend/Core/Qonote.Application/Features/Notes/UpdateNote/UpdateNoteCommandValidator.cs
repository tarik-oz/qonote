using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Notes.UpdateNote;

public sealed class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.CustomTitle ?? string.Empty)
            .TrimmedMaxLength(200, "Title must not exceed 200 characters.")
            .TrimmedMatches(@"^[\p{L}\p{N}\p{P}\p{S} ]+$", "Title can only contain letters (including Turkish), numbers, punctuation, symbols, and spaces.")
            .When(x => x.CustomTitle is not null);

        RuleFor(x => x.UserLink1).MaximumLength(500);
        RuleFor(x => x.UserLink2).MaximumLength(500);
        RuleFor(x => x.UserLink3).MaximumLength(500);

        // Ensure at least one updatable field is present
        RuleFor(x => x)
            .Must(x => x.CustomTitle != null || x.IsPublic != null || x.UserLink1 != null || x.UserLink2 != null || x.UserLink3 != null)
            .WithMessage("No fields provided to update.");
    }
}

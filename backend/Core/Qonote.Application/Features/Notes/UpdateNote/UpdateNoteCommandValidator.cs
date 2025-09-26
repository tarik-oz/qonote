using FluentValidation;

namespace Qonote.Core.Application.Features.Notes.UpdateNote;

public sealed class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.CustomTitle).MaximumLength(200);
        RuleFor(x => x.UserLink1).MaximumLength(500);
        RuleFor(x => x.UserLink2).MaximumLength(500);
        RuleFor(x => x.UserLink3).MaximumLength(500);

        // Ensure at least one updatable field is present
        RuleFor(x => x)
            .Must(x => x.CustomTitle != null || x.IsPublic != null || x.UserLink1 != null || x.UserLink2 != null || x.UserLink3 != null)
            .WithMessage("No fields provided to update.");
    }
}

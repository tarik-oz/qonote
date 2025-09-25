using FluentValidation;

namespace Qonote.Core.Application.Features.Notes.AssignGroup;

public sealed class AssignNoteGroupCommandValidator : AbstractValidator<AssignNoteGroupCommand>
{
    public AssignNoteGroupCommandValidator()
    {
        RuleFor(x => x.NoteId).GreaterThan(0);
        // NoteGroupId can be null to unassign; if provided, must be > 0
        When(x => x.NoteGroupId.HasValue, () =>
        {
            RuleFor(x => x.NoteGroupId!.Value).GreaterThan(0);
        });
    }
}

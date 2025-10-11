using FluentValidation;

namespace Qonote.Core.Application.Features.Sections.SetSectionUiStateBatch;

public sealed class SetSectionUiStateBatchCommandValidator : AbstractValidator<SetSectionUiStateBatchCommand>
{
    public SetSectionUiStateBatchCommandValidator()
    {
        RuleFor(x => x.NoteId).GreaterThan(0);
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items are required.")
            .Must(items => items.Count > 0).WithMessage("Items must not be empty.");
        RuleForEach(x => x.Items)
            .ChildRules(r =>
            {
                r.RuleFor(i => i.SectionId).GreaterThan(0);
            });
    }
}

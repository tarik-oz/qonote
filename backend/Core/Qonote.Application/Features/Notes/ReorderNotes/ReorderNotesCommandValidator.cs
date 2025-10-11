using FluentValidation;

namespace Qonote.Core.Application.Features.Notes.ReorderNotes;

public sealed class ReorderNotesCommandValidator : AbstractValidator<ReorderNotesCommand>
{
    public ReorderNotesCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(child =>
        {
            child.RuleFor(i => i.Id).GreaterThan(0);
            child.RuleFor(i => i.Order).GreaterThanOrEqualTo(0);
        });

        RuleFor(x => x.Items.Select(i => i.Id))
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithMessage("Ids must be distinct.");

        RuleFor(x => x.Items.Select(i => i.Order))
            .Must(orders => orders.Distinct().Count() == orders.Count())
            .WithMessage("Orders must be distinct.");
    }
}

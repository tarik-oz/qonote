using FluentValidation;

namespace Qonote.Core.Application.Features.Sections.ReorderSections;

public sealed class ReorderSectionsCommandValidator : AbstractValidator<ReorderSectionsCommand>
{
    public ReorderSectionsCommandValidator()
    {
        RuleFor(x => x.NoteId).GreaterThan(0);
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items are required.")
            .Must(items => items.Count > 0).WithMessage("Items must not be empty.")
            .Must(items => items.Select(i => i.Id).Distinct().Count() == items.Count)
            .WithMessage("Duplicate section ids in payload.")
            .Must(items => items.Select(i => i.Order).Distinct().Count() == items.Count)
            .WithMessage("Duplicate orders in payload.");
        RuleForEach(x => x.Items)
            .ChildRules(r =>
            {
                r.RuleFor(i => i.Id).GreaterThan(0);
                r.RuleFor(i => i.Order).GreaterThanOrEqualTo(0);
            });
    }
}

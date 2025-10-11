using FluentValidation;

namespace Qonote.Core.Application.Features.Notes.ListFlatNotes;

public sealed class ListFlatNotesQueryValidator : AbstractValidator<ListFlatNotesQuery>
{
    public ListFlatNotesQueryValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .When(x => x.Limit.HasValue);

        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Offset.HasValue);
    }
}

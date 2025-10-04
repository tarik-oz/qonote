using FluentValidation;

namespace Qonote.Core.Application.Features.Notes.SearchNotes;

public sealed class SearchNotesQueryValidator : AbstractValidator<SearchNotesQuery>
{
    public SearchNotesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}



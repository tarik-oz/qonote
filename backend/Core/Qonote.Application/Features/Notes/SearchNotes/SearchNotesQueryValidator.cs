using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Notes.SearchNotes;

public sealed class SearchNotesQueryValidator : AbstractValidator<SearchNotesQuery>
{
    public SearchNotesQueryValidator()
    {
        RuleFor(x => x.Query ?? string.Empty)
            .TrimmedMaxLength(200, "Query must not exceed 200 characters.");
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}

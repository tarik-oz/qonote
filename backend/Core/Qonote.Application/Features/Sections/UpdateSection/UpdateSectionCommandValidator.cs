using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Sections.UpdateSection;

public sealed class UpdateSectionCommandValidator : AbstractValidator<UpdateSectionCommand>
{
    public UpdateSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title ?? string.Empty)
            .TrimmedMaxLength(200, "Title must not exceed 200 characters.")
            .TrimmedMatches(@"^[\p{L}\p{N}\p{P}\p{S} ]+$", "Title can only contain letters (including Turkish), numbers, punctuation, symbols, and spaces.")
            .When(x => x.Title is not null);
        // For non-Timestamped, forbid Start/End
        When(x => x.Type is not null && x.Type.Value != Core.Domain.Enums.SectionType.Timestamped, () =>
        {
            RuleFor(x => x.StartTime).Must(st => st is null).WithMessage("Only Timestamped sections can have StartTime.");
            RuleFor(x => x.EndTime).Must(et => et is null).WithMessage("Only Timestamped sections can have EndTime.");
        });
    }
}

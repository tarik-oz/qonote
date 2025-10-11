using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Sections.CreateSection;

public sealed class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
{
    public CreateSectionCommandValidator()
    {
        RuleFor(x => x.NoteId).GreaterThan(0);
        RuleFor(x => x.Title ?? string.Empty)
            .TrimmedMaxLength(200, "Title must not exceed 200 characters.")
            .TrimmedNotEmpty("Title must not be empty.")
            .TrimmedMatches(@"^[\p{L}\p{N}\p{P}\p{S} ]+$", "Title can only contain letters (including Turkish), numbers, punctuation, symbols, and spaces.")
            .When(x => x.Title is not null);
        // If Type provided and not Timestamped, reject time fields
        RuleFor(x => x)
            .Must(x => x.Type is null || x.Type.Value == Core.Domain.Enums.SectionType.Timestamped || (x.StartTime is null && x.EndTime is null))
            .WithMessage("Only Timestamped sections can have StartTime/EndTime.");
    }
}

using FluentValidation;

namespace Qonote.Core.Application.Features.Sections.CreateSection;

public sealed class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
{
    public CreateSectionCommandValidator()
    {
        RuleFor(x => x.NoteId).GreaterThan(0);
        RuleFor(x => x.Title).MaximumLength(200);
        // If Type provided and not Timestamped, reject time fields
        RuleFor(x => x)
            .Must(x => x.Type is null || x.Type.Value == Core.Domain.Enums.SectionType.Timestamped || (x.StartTime is null && x.EndTime is null))
            .WithMessage("Only Timestamped sections can have StartTime/EndTime.");
    }
}

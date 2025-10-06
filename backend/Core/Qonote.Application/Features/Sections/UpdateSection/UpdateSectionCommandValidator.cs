using FluentValidation;

namespace Qonote.Core.Application.Features.Sections.UpdateSection;

public sealed class UpdateSectionCommandValidator : AbstractValidator<UpdateSectionCommand>
{
    public UpdateSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).MaximumLength(200);
        // For non-Timestamped, forbid Start/End
        When(x => x.Type is not null && x.Type.Value != Core.Domain.Enums.SectionType.Timestamped, () =>
        {
            RuleFor(x => x.StartTime).Must(st => st is null).WithMessage("Only Timestamped sections can have StartTime.");
            RuleFor(x => x.EndTime).Must(et => et is null).WithMessage("Only Timestamped sections can have EndTime.");
        });
    }
}

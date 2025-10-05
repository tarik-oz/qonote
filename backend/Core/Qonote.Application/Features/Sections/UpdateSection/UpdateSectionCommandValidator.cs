using FluentValidation;

namespace Qonote.Core.Application.Features.Sections.UpdateSection;

public sealed class UpdateSectionCommandValidator : AbstractValidator<UpdateSectionCommand>
{
    public UpdateSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).MaximumLength(200);
    }
}

using FluentValidation;

namespace Qonote.Core.Application.Features.Sections.DeleteSection;

public sealed class DeleteSectionCommandValidator : AbstractValidator<DeleteSectionCommand>
{
    public DeleteSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

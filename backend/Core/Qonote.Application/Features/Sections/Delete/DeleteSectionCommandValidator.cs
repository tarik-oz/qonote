using FluentValidation;

namespace Qonote.Core.Application.Features.Sections.Delete;

public sealed class DeleteSectionCommandValidator : AbstractValidator<DeleteSectionCommand>
{
    public DeleteSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

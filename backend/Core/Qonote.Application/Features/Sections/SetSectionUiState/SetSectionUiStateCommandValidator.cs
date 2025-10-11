using FluentValidation;

namespace Qonote.Core.Application.Features.Sections.SetSectionUiState;

public sealed class SetSectionUiStateCommandValidator : AbstractValidator<SetSectionUiStateCommand>
{
    public SetSectionUiStateCommandValidator()
    {
        RuleFor(x => x.SectionId).GreaterThan(0);
    }
}

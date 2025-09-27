using FluentValidation;

namespace Qonote.Core.Application.Features.Sections.Create;

public sealed class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
{
    public CreateSectionCommandValidator()
    {
        RuleFor(x => x.NoteId).GreaterThan(0);
        RuleFor(x => x.Title).MaximumLength(200);
    }
}

using FluentValidation;

namespace Qonote.Core.Application.Features.NoteGroups.RenameNoteGroup;

public sealed class RenameNoteGroupCommandValidator : AbstractValidator<RenameNoteGroupCommand>
{
    public RenameNoteGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

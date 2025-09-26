using FluentValidation;

namespace Qonote.Core.Application.Features.NoteGroups.DeleteNoteGroup;

public sealed class DeleteNoteGroupCommandValidator : AbstractValidator<DeleteNoteGroupCommand>
{
    public DeleteNoteGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

using FluentValidation;

namespace Qonote.Core.Application.Features.Notes.DeleteNote;

public sealed class DeleteNoteCommandValidator : AbstractValidator<DeleteNoteCommand>
{
    public DeleteNoteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

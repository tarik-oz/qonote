using FluentValidation;

namespace Qonote.Core.Application.Features.NoteGroups.CreateNoteGroup;

public sealed class CreateNoteGroupCommandValidator : AbstractValidator<CreateNoteGroupCommand>
{
    public CreateNoteGroupCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

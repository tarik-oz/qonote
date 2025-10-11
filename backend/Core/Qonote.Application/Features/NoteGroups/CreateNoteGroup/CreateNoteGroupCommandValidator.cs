using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.NoteGroups.CreateNoteGroup;

public sealed class CreateNoteGroupCommandValidator : AbstractValidator<CreateNoteGroupCommand>
{
    public CreateNoteGroupCommandValidator()
    {
        RuleFor(x => x.Title)
            .TrimmedNotEmpty("Title is required.")
            .TrimmedMaxLength(200, "Title must not exceed 200 characters.")
            .TrimmedMatches(@"^[\p{L}\p{N}\p{P}\p{S} ]+$", "Title can only contain letters (including Turkish), numbers, punctuation, symbols, and spaces.");
    }
}

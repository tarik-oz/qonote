using FluentValidation;

namespace Qonote.Core.Application.Features.Blocks.UpdateBlock;

public sealed class UpdateBlockCommandValidator : AbstractValidator<UpdateBlockCommand>
{
    public UpdateBlockCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Content)
            .Must(c => c is null || c.Trim().Length <= 5000)
            .WithMessage("Content must not exceed 5000 characters.");
    }
}

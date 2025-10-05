using FluentValidation;

namespace Qonote.Core.Application.Features.Blocks.UpdateBlock;

public sealed class UpdateBlockCommandValidator : AbstractValidator<UpdateBlockCommand>
{
    public UpdateBlockCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Content).MaximumLength(5000);
    }
}

using FluentValidation;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.DeletePromoCode;

public sealed class DeletePromoCodeCommandValidator : AbstractValidator<DeletePromoCodeCommand>
{
    public DeletePromoCodeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

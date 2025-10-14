using FluentValidation;

namespace Qonote.Core.Application.Features.Subscriptions.RedeemPromoCode;

public sealed class RedeemPromoCodeCommandValidator : AbstractValidator<RedeemPromoCodeCommand>
{
    public RedeemPromoCodeCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(64)
            .Matches("^[A-Za-z0-9-]+$")
            .WithMessage("Code may contain only alphanumeric characters and dashes.");
    }
}

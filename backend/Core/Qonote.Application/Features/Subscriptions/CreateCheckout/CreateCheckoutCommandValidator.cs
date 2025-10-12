using FluentValidation;

namespace Qonote.Core.Application.Features.Subscriptions.CreateCheckout;

public sealed class CreateCheckoutCommandValidator : AbstractValidator<CreateCheckoutCommand>
{
    public CreateCheckoutCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .GreaterThan(0);

        RuleFor(x => x.BillingInterval)
            .IsInEnum();

        RuleFor(x => x.SuccessUrl)
            .Must(BeValidHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.SuccessUrl))
            .WithMessage("SuccessUrl must be an absolute http(s) URL.");

        RuleFor(x => x.CancelUrl)
            .Must(BeValidHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.CancelUrl))
            .WithMessage("CancelUrl must be an absolute http(s) URL.");
    }

    private static bool BeValidHttpUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
        return false;
    }
}

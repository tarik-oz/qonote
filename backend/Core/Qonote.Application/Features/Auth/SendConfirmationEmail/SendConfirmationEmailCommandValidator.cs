using FluentValidation;

namespace Qonote.Core.Application.Features.Auth.SendConfirmationEmail;

public sealed class SendConfirmationEmailCommandValidator : AbstractValidator<SendConfirmationEmailCommand>
{
    public SendConfirmationEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

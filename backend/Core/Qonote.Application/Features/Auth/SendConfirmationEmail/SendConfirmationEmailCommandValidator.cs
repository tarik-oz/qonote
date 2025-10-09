using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Auth.SendConfirmationEmail;

public sealed class SendConfirmationEmailCommandValidator : AbstractValidator<SendConfirmationEmailCommand>
{
    public SendConfirmationEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .TrimmedNotEmpty("Email is required.")
            .TrimmedEmail("A valid email address is required.");
    }
}

using FluentValidation;
using Qonote.Core.Application.Extensions;

namespace Qonote.Core.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .TrimmedNotEmpty("Email is required.")
            .TrimmedEmail("Invalid email address.");
    }
}

using FluentValidation;

namespace Qonote.Core.Application.Features.Users.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(p => p.OldPassword)
            .NotEmpty().WithMessage("Current password cannot be empty.");

        RuleFor(p => p.NewPassword)
            .NotEmpty().WithMessage("New password cannot be empty.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long.")
            .NotEqual(p => p.OldPassword).WithMessage("New password cannot be the same as the old password.");

        RuleFor(p => p.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm new password cannot be empty.")
            .Equal(p => p.NewPassword).WithMessage("The new passwords do not match.");
    }
}

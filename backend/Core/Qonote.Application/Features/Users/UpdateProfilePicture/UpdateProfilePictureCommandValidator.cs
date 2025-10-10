using FluentValidation;

namespace Qonote.Core.Application.Features.Users.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommandValidator : AbstractValidator<UpdateProfilePictureCommand>
{
    public UpdateProfilePictureCommandValidator()
    {
        RuleFor(x => x.ProfilePicture)
            .NotNull()
            .WithMessage("A profile picture file is required.");

        RuleFor(x => x.ProfilePicture.Length)
            .LessThanOrEqualTo(2 * 1024 * 1024) // 2 MB
            .WithMessage("Profile picture size must not exceed 2 MB.");

        RuleFor(x => x.ProfilePicture.ContentType)
            .Must(ct => ct is "image/jpeg" or "image/png" or "image/webp")
            .WithMessage("Only .jpg, .png, or .webp image formats are allowed.");
    }
}

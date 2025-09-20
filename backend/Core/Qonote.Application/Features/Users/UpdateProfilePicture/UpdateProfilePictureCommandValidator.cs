using FluentValidation;

namespace Qonote.Core.Application.Features.Users.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommandValidator : AbstractValidator<UpdateProfilePictureCommand>
{
    public UpdateProfilePictureCommandValidator()
    {
        RuleFor(x => x.ProfilePicture)
            .NotEmpty()
            .WithMessage("A profile picture file is required.");

        RuleFor(x => x.ProfilePicture.Length)
            .LessThanOrEqualTo(2 * 1024 * 1024) // 2 MB
            .WithMessage("Profile picture size must not exceed 2 MB.");

        RuleFor(x => x.ProfilePicture.ContentType)
            .Must(x => x.Equals("image/jpeg") || x.Equals("image/png"))
            .WithMessage("Only .jpg and .png image formats are allowed.");
    }
}

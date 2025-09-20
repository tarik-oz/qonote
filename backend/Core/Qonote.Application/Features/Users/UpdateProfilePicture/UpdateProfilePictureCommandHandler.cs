using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Storage;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Users.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommandHandler : IRequestHandler<UpdateProfilePictureCommand, string>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private const string ProfilePicturesContainer = "profile-pictures";

    public UpdateProfilePictureCommandHandler(
        UserManager<ApplicationUser> userManager, 
        ICurrentUserService currentUserService, 
        IFileStorageService fileStorageService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<string> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User not found.");

        // Delete the old profile picture if it exists
        if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            await _fileStorageService.DeleteAsync(user.ProfileImageUrl, ProfilePicturesContainer);
        }

        // Generate a consistent file name for the user
        var fileName = userId + Path.GetExtension(request.ProfilePicture.FileName);

        // Upload the new profile picture
        var newImageUrl = await _fileStorageService.UploadAsync(request.ProfilePicture, ProfilePicturesContainer, fileName);

        // Update the user's profile image URL in the database
        user.ProfileImageUrl = newImageUrl;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new Exception("Failed to update user profile picture URL.");

        return newImageUrl;
    }
}

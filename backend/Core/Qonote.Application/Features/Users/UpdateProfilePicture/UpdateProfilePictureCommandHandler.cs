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
        // The UserMustExistRule is now handled by the BusinessRulesBehavior.
        var user = await _userManager.FindByIdAsync(_currentUserService.UserId!);

        if (!string.IsNullOrWhiteSpace(user!.ProfileImageUrl))
        {
            await _fileStorageService.DeleteAsync(user.ProfileImageUrl, ProfilePicturesContainer);
        }

        var fileName = user.Id + Path.GetExtension(request.ProfilePicture.FileName);
        var newImageUrl = await _fileStorageService.UploadAsync(request.ProfilePicture, ProfilePicturesContainer, fileName);

        user.ProfileImageUrl = newImageUrl;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));
        }

        return newImageUrl;
    }
}

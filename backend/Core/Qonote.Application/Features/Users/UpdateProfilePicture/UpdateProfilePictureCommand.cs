using MediatR;
using Microsoft.AspNetCore.Http;

namespace Qonote.Core.Application.Features.Users.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommand : IRequest<string> // Returns the new URL of the picture
{
    public IFormFile ProfilePicture { get; set; } = null!;
}

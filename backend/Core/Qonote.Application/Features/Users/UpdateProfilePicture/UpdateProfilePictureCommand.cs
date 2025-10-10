using MediatR;
using Microsoft.AspNetCore.Http;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Users.UpdateProfilePicture;

public sealed record UpdateProfilePictureCommand : IRequest<string>, IAuthenticatedRequest // Returns the new URL of the picture
{
    public IFormFile ProfilePicture { get; init; } = null!;
}

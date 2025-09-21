using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Users.UpdateProfileInfo;

public sealed class UpdateProfileInfoCommand : IRequest<Unit>, IAuthenticatedRequest
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
}

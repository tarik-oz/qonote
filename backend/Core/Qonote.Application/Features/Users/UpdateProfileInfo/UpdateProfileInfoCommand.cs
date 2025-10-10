using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Users.UpdateProfileInfo;

public sealed record UpdateProfileInfoCommand(
    string Name,
    string Surname
) : IRequest<Unit>, IAuthenticatedRequest;

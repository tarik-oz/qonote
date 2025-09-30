using MediatR;

namespace Qonote.Core.Application.Features.Users.GetMe;

public sealed record GetMeQuery : IRequest<GetMeDto>;

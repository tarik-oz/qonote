using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Auth.Logout;

public sealed record LogoutCommand() : IRequest<Unit>, IAuthenticatedRequest;


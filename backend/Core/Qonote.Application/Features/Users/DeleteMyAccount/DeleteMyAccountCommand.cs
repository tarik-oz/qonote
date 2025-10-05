using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Users.DeleteMyAccount;

public sealed record DeleteMyAccountCommand() : IRequest<Unit>, IAuthenticatedRequest;



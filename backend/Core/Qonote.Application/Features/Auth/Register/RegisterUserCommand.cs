using MediatR;

namespace Qonote.Core.Application.Features.Auth.Register;

public record RegisterUserCommand(
    string Name,
    string Surname,
    string Email,
    string Password) : IRequest<Unit>;

using MediatR;

namespace Qonote.Core.Application.Features.Auth.Commands.Register;

public class RegisterUserCommand : IRequest<string>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

using MediatR;
using Qonote.Core.Application.Features.Auth._Shared;

namespace Qonote.Core.Application.Features.Auth.Register;

public class RegisterUserCommand : IRequest<LoginResponseDto>
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

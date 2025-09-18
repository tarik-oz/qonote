using MediatR;

namespace Qonote.Core.Application.Features.Users.UpdateProfileInfo;

public sealed class UpdateProfileInfoCommand : IRequest<Unit>
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
}

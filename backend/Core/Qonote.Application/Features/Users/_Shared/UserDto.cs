namespace Qonote.Core.Application.Features.Users._Shared;

public sealed record UserDto(
    string Id,
    string Name,
    string Surname,
    string Email,
    string? ProfileImageUrl
)
{
    public string FullName => $"{Name} {Surname}".Trim();
}

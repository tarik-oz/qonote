namespace Qonote.Core.Application.Features.Users._Shared;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string FullName => $"{Name} {Surname}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
}

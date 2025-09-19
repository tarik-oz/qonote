namespace Qonote.Core.Application.Abstractions.Security;

public class ExternalLoginUserDto
{
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

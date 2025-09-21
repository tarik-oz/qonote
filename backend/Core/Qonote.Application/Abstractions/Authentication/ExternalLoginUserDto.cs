namespace Qonote.Core.Application.Abstractions.Authentication;

public class ExternalLoginUserDto
{
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

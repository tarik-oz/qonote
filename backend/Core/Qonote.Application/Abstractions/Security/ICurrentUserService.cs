namespace Qonote.Core.Application.Abstractions.Security;

public interface ICurrentUserService
{
    string? UserId { get; }
}
namespace Qonote.Core.Application.Abstractions.Requests;

/// <summary>
/// Marker interface for requests that require the user to be authenticated.
/// This allows a generic business rule to check for user existence.
/// </summary>
public interface IAuthenticatedRequest
{
}

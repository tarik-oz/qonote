using System.Security.Claims;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Abstractions.Security;

public interface ITokenService
{
    (string token, DateTime expiry) CreateAccessToken(ApplicationUser user, IList<string> roles);

    string GenerateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromExpiredAccessToken(string accessToken);
}

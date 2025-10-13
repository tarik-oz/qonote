using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Identity;

namespace Qonote.Infrastructure.Infrastructure.Security;

public class TokenService : ITokenService
{
    private readonly TokenSettings _tokenSettings;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IOptions<TokenSettings> options, ILogger<TokenService> logger)
    {
        _tokenSettings = options.Value;
        _logger = logger;
    }

    // Pasaport basma işlemi
    public (string token, DateTime expiry) CreateAccessToken(ApplicationUser user, IList<string> roles)
    {
        _logger.LogInformation("CreateAccessToken. userId={UserId}, roles={RolesCount}", user.Id, roles?.Count ?? 0);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new("fullName", $"{user.Name} {user.Surname}".Trim()),
        };

        if (roles is not null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryDate = DateTime.UtcNow.AddMinutes(_tokenSettings.TokenValidityInMinutes);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _tokenSettings.Issuer,
            audience: _tokenSettings.Audience,
            expires: expiryDate,
            claims: claims,
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(tokenDescriptor);

        _logger.LogDebug("Access token created. userId={UserId}, expiresAtUtc={Expiry}", user.Id, expiryDate);
        return (tokenString, expiryDate);
    }

    public (string token, DateTime expiry) CreateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var token = Convert.ToBase64String(randomNumber);
        var expiry = DateTime.UtcNow.AddDays(_tokenSettings.RefreshTokenValidityInDays);
        _logger.LogDebug("Refresh token created. expiresAtUtc={Expiry}", expiry);
        return (token, expiry);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredAccessToken(string accessToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateActor = false,
            ValidateLifetime = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Secret))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        _logger.LogDebug("GetPrincipalFromExpiredAccessToken called.");
        var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogWarning("Invalid token detected during principal extraction.");
            throw new SecurityTokenException("Invalid token.");
        }

        return principal;
    }
}

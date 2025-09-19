namespace Qonote.Core.Application.Abstractions.Security;

public interface IGoogleAuthService
{
    string GenerateAuthUrl(string redirectUri);

    Task<ExternalLoginUserDto> ExchangeCodeForUserInfoAsync(string code, string redirectUri);
}

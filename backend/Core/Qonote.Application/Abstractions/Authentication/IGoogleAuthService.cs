namespace Qonote.Core.Application.Abstractions.Authentication;

public interface IGoogleAuthService
{
    string GenerateAuthUrl(string redirectUri);

    Task<ExternalLoginUserDto> ExchangeCodeForUserInfoAsync(string code, string redirectUri);
}

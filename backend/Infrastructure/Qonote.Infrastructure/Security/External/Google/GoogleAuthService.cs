using System.Text.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Infrastructure.Infrastructure.Security;

namespace Qonote.Infrastructure.Security.External.Google;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleSettings _googleSettings;
    private readonly HttpClient _httpClient;

    public GoogleAuthService(IOptions<GoogleSettings> googleSettings, HttpClient httpClient)
    {
        _googleSettings = googleSettings.Value;
        _httpClient = httpClient;
    }

    public string GenerateAuthUrl(string redirectUri)
    {
        const string googleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";

        var parameters = new Dictionary<string, string?>
        {
            { "client_id", _googleSettings.ClientId },
            { "redirect_uri", redirectUri },
            { "response_type", "code" },
            { "scope", "openid email profile" },
            { "access_type", "offline" }
        };

        return Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(googleAuthUrl, parameters);
    }

    public async Task<ExternalLoginUserDto> ExchangeCodeForUserInfoAsync(string code, string redirectUri)
    {
        var tokenRequest = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", _googleSettings.ClientId },
            { "client_secret", _googleSettings.ClientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        };

        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenRequest));

        if (!response.IsSuccessStatusCode)
        {
            // In a real app, log the response content for debugging
            throw new Exception("Failed to retrieve token from Google.");
        }

        var content = await response.Content.ReadAsStringAsync();
        var googleTokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(content);

        if (googleTokenResponse is null || string.IsNullOrWhiteSpace(googleTokenResponse.IdToken))
        {
            throw new Exception("Failed to deserialize or get id_token from Google response.");
        }

        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleSettings.ClientId }
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleTokenResponse.IdToken, validationSettings);

            return new ExternalLoginUserDto
            {
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                ProfilePictureUrl = payload.Picture
            };
        }
        catch (InvalidJwtException ex)
        {
            // In a real app, log the exception details
            throw new Exception("Invalid Google ID token.", ex);
        }
    }
}

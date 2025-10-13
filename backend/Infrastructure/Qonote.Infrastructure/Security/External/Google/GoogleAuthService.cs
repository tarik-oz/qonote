using System.Text.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qonote.Core.Application.Abstractions.Authentication;

namespace Qonote.Infrastructure.Infrastructure.Security.External.Google;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleSettings _googleSettings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(IOptions<GoogleSettings> googleSettings, HttpClient httpClient, ILogger<GoogleAuthService> logger)
    {
        _googleSettings = googleSettings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public string GenerateAuthUrl(string redirectUri)
    {
        const string googleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        var effectiveRedirectUri = string.IsNullOrWhiteSpace(redirectUri) ? _googleSettings.RedirectUri : redirectUri;
        if (string.IsNullOrWhiteSpace(effectiveRedirectUri))
        {
            throw new InvalidOperationException("Google RedirectUri is not configured. Set GoogleSettings:RedirectUri or pass a redirectUri parameter.");
        }

        var parameters = new Dictionary<string, string?>
        {
            { "client_id", _googleSettings.ClientId },
            { "redirect_uri", effectiveRedirectUri },
            { "response_type", "code" },
            { "scope", "openid email profile" },
            { "access_type", "offline" }
        };

        var url = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(googleAuthUrl, parameters);
        _logger.LogInformation("Google auth URL generated.");
        return url;
    }

    public async Task<ExternalLoginUserDto> ExchangeCodeForUserInfoAsync(string code, string redirectUri)
    {
        var effectiveRedirectUri = string.IsNullOrWhiteSpace(redirectUri) ? _googleSettings.RedirectUri : redirectUri;
        if (string.IsNullOrWhiteSpace(effectiveRedirectUri))
        {
            throw new InvalidOperationException("Google RedirectUri is not configured. Set GoogleSettings:RedirectUri or pass a redirectUri parameter.");
        }
        var tokenRequest = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", _googleSettings.ClientId },
            { "client_secret", _googleSettings.ClientSecret },
            { "redirect_uri", effectiveRedirectUri },
            { "grant_type", "authorization_code" }
        };

        _logger.LogInformation("Google token exchange started.");
        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenRequest));

        if (!response.IsSuccessStatusCode)
        {
            var contentError = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Google token endpoint responded with {StatusCode}: {Body}", (int)response.StatusCode, contentError);
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

            _logger.LogInformation("Google ID token validated. emailPresent={HasEmail}", !string.IsNullOrWhiteSpace(payload.Email));
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
            _logger.LogWarning(ex, "Invalid Google ID token.");
            throw new Exception("Invalid Google ID token.", ex);
        }
    }
}

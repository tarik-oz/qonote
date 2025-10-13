using System.Text.Json.Serialization;

namespace Qonote.Infrastructure.Infrastructure.Security.External.Google;

public sealed class GoogleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}

namespace DotMatchLens.Football.Services;

/// <summary>
/// Configuration options for the football-data.org API client.
/// </summary>
public sealed class FootballDataApiOptions
{
    public const string SectionName = "FootballDataApi";

    /// <summary>
    /// The base URL for the football-data.org API.
    /// </summary>
    public Uri BaseUrl { get; set; } = new Uri("http://api.football-data.org/v4");

    /// <summary>
    /// The API token for authentication (optional for free tier with rate limits).
    /// </summary>
    public string? ApiToken { get; set; }

    /// <summary>
    /// Rate limit per minute for API calls.
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 10;

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int Timeout { get; set; } = 30;
}

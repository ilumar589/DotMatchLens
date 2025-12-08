using DotMatchLens.Football.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotMatchLens.Football.HealthChecks;

/// <summary>
/// Health check for the football-data.org API connectivity.
/// </summary>
public sealed class FootballDataApiHealthCheck : IHealthCheck
{
    private readonly FootballDataApiClient _apiClient;

    public FootballDataApiHealthCheck(FootballDataApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use a lightweight API call to check connectivity
            // GetCompetitionAsync returns null on errors, so we check for that
            var result = await _apiClient.GetCompetitionAsync("PL", cancellationToken);

            if (result is not null)
            {
                return HealthCheckResult.Healthy(
                    "Football Data API is available.",
                    data: new Dictionary<string, object>
                    {
                        ["Provider"] = "football-data.org"
                    });
            }

            // If result is null, the API had an error (logged by the client)
            return HealthCheckResult.Degraded(
                "Football Data API returned an error or is unavailable.",
                data: new Dictionary<string, object>
                {
                    ["Provider"] = "football-data.org"
                });
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Degraded(
                "Football Data API request timed out.",
                data: new Dictionary<string, object>
                {
                    ["Provider"] = "football-data.org"
                });
        }
        catch (Exception)
        {
            return HealthCheckResult.Degraded(
                "Cannot connect to Football Data API.",
                data: new Dictionary<string, object>
                {
                    ["Provider"] = "football-data.org"
                });
        }
    }
}

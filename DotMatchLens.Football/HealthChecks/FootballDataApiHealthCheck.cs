using DotMatchLens.Football.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotMatchLens.Football.HealthChecks;

/// <summary>
/// Health check for the football-data.org API connectivity.
/// Returns Degraded instead of failing on errors since this is a non-critical external service.
/// </summary>
public sealed class FootballDataApiHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;

    public FootballDataApiHealthCheck(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a scope to resolve the scoped FootballDataApiClient
            using var scope = _serviceProvider.CreateScope();
            var apiClient = scope.ServiceProvider.GetService<FootballDataApiClient>();

            if (apiClient is null)
            {
                return HealthCheckResult.Degraded(
                    "Football Data API client not configured.",
                    data: new Dictionary<string, object>
                    {
                        ["Provider"] = "football-data.org"
                    });
            }

            // Use a short timeout for health check
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            var result = await apiClient.GetCompetitionAsync("PL", timeoutCts.Token);

            if (result is not null)
            {
                return HealthCheckResult.Healthy(
                    "Football Data API is available.",
                    data: new Dictionary<string, object>
                    {
                        ["Provider"] = "football-data.org"
                    });
            }

            return HealthCheckResult.Degraded(
                "Football Data API returned an error or is unavailable.",
                data: new Dictionary<string, object>
                {
                    ["Provider"] = "football-data.org"
                });
        }
        catch (OperationCanceledException)
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

using DotMatchLens.Data.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotMatchLens.Data.HealthChecks;

/// <summary>
/// Health check for the Football database connectivity.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseHealthCheck(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a scope to resolve the scoped DbContext
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<FootballDbContext>();

            if (dbContext is null)
            {
                return HealthCheckResult.Unhealthy(
                    "Database context not configured.",
                    data: new Dictionary<string, object>
                    {
                        ["Provider"] = "PostgreSQL"
                    });
            }

            // Use a short timeout for health check
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

            var canConnect = await dbContext.Database.CanConnectAsync(timeoutCts.Token);

            return canConnect
                ? HealthCheckResult.Healthy("Database connection is healthy.")
                : HealthCheckResult.Unhealthy("Cannot connect to database.");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy(
                "Database connection timed out.",
                data: new Dictionary<string, object>
                {
                    ["Provider"] = "PostgreSQL"
                });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database connection failed.",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["Provider"] = "PostgreSQL"
                });
        }
    }
}

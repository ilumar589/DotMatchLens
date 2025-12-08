using DotMatchLens.Data.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotMatchLens.Data.HealthChecks;

/// <summary>
/// Health check for the Football database connectivity.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly FootballDbContext _context;

    public DatabaseHealthCheck(FootballDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple connectivity check using EF Core
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("Database connection is healthy.")
                : HealthCheckResult.Unhealthy("Cannot connect to database.");
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

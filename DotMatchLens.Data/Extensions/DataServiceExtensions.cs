using DotMatchLens.Data.Context;
using DotMatchLens.Data.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace DotMatchLens.Data.Extensions;

/// <summary>
/// Extension methods for registering Data module services.
/// </summary>
public static class DataServiceExtensions
{
    /// <summary>
    /// Adds the Football database context to the service collection.
    /// Uses Aspire's PostgreSQL integration with Entity Framework Core.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="connectionName">The name of the database connection (from Aspire).</param>
    /// <returns>The builder for chaining.</returns>
    public static IHostApplicationBuilder AddFootballDatabase(
        this IHostApplicationBuilder builder,
        string connectionName = "footballdb")
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Use Aspire's PostgreSQL integration
        builder.AddNpgsqlDbContext<FootballDbContext>(connectionName, configureDbContextOptions: options =>
        {
            options.UseNpgsql(npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
            });
        });

        // Register database health check
        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(
                "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db"],
                timeout: TimeSpan.FromSeconds(5));

        return builder;
    }
}

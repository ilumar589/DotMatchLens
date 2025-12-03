using DotMatchLens.Data.Context;
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

        return builder;
    }
}

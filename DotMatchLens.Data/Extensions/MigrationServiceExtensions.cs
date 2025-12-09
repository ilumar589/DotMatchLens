using DotMatchLens.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotMatchLens.Data.Extensions;

/// <summary>
/// Extension methods for running database migrations at application startup.
/// </summary>
public static class MigrationServiceExtensions
{
    /// <summary>
    /// Adds a hosted service that runs EF Core migrations at application startup.
    /// </summary>
    public static IHostApplicationBuilder AddDatabaseMigrations(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHostedService<MigrationHostedService>();
        
        return builder;
    }

    /// <summary>
    /// Hosted service that runs database migrations before the application starts accepting requests.
    /// </summary>
    private sealed class MigrationHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationHostedService> _logger;

        public MigrationHostedService(
            IServiceProvider serviceProvider,
            ILogger<MigrationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            MigrationLogMessages.LogMigrationStarted(_logger);
            
            try
            {
                // Create a scope to get the DbContext
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FootballDbContext>();
                
                // Apply pending migrations
                await context.Database.MigrateAsync(cancellationToken);
                
                MigrationLogMessages.LogMigrationCompleted(_logger);
            }
            catch (Exception ex)
            {
                MigrationLogMessages.LogMigrationError(_logger, ex);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

/// <summary>
/// Log messages for database migrations using source generator pattern.
/// </summary>
internal static partial class MigrationLogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Running database migrations...")]
    public static partial void LogMigrationStarted(ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Database migrations completed successfully")]
    public static partial void LogMigrationCompleted(ILogger logger);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "An error occurred while running database migrations")]
    public static partial void LogMigrationError(ILogger logger, Exception exception);
}

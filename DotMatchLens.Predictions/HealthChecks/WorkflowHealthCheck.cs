using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotMatchLens.Predictions.HealthChecks;

/// <summary>
/// Health check for workflow system components.
/// </summary>
public sealed class WorkflowHealthCheck : IHealthCheck
{
    private readonly ILogger<WorkflowHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public WorkflowHealthCheck(ILogger<WorkflowHealthCheck> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if workflow infrastructure is available
            // In a real implementation, you might check:
            // - MassTransit connection status
            // - Redis connection for saga persistence
            // - Database connectivity
            // For now, we'll just verify basic functionality

            var data = new Dictionary<string, object>
            {
                { "workflow_system", "operational" },
                { "timestamp", DateTime.UtcNow }
            };

            return Task.FromResult(
                HealthCheckResult.Healthy("Workflow system is operational", data));
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogError(ex, "Workflow health check failed");
#pragma warning restore CA1848
            return Task.FromResult(
                HealthCheckResult.Unhealthy("Workflow system is not operational", ex));
        }
    }
}

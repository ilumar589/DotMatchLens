namespace DotMatchLens.Predictions.Configuration;

/// <summary>
/// Configuration options for workflow telemetry and visualization.
/// </summary>
public sealed class WorkflowOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Workflow";

    /// <summary>
    /// Gets or sets a value indicating whether telemetry is enabled.
    /// </summary>
    public bool EnableTelemetry { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether workflow visualization is enabled.
    /// </summary>
    public bool EnableVisualization { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether Server-Sent Events (SSE) for real-time updates are enabled.
    /// </summary>
    public bool EnableSseEvents { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of workflow events to retain in memory for visualization.
    /// </summary>
    public int MaxEventsToRetain { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the interval in seconds for health check execution.
    /// </summary>
    public int HealthCheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the timeout in seconds for Ollama health check.
    /// </summary>
    public int OllamaHealthCheckTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether detailed metrics are enabled.
    /// </summary>
    public bool EnableDetailedMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether distributed tracing is enabled.
    /// </summary>
    public bool EnableDistributedTracing { get; set; } = true;

    /// <summary>
    /// Gets or sets the sampling rate for distributed tracing (0.0 to 1.0).
    /// </summary>
    public double TracingSamplingRate { get; set; } = 1.0;
}

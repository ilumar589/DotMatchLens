using System.Diagnostics;

namespace DotMatchLens.Predictions.Telemetry;

/// <summary>
/// ActivitySource instances for workflow telemetry and distributed tracing.
/// </summary>
public static class WorkflowActivitySources
{
    /// <summary>
    /// ActivitySource name for workflow operations.
    /// </summary>
    public const string WorkflowSourceName = "DotMatchLens.Predictions.Workflow";

    /// <summary>
    /// ActivitySource name for agent operations.
    /// </summary>
    public const string AgentSourceName = "DotMatchLens.Predictions.Agent";

    /// <summary>
    /// ActivitySource name for MassTransit operations.
    /// </summary>
    public const string MassTransitSourceName = "DotMatchLens.Predictions.MassTransit";

    /// <summary>
    /// ActivitySource for workflow operations.
    /// </summary>
    public static readonly ActivitySource Workflow = new(WorkflowSourceName, "1.0.0");

    /// <summary>
    /// ActivitySource for agent operations.
    /// </summary>
    public static readonly ActivitySource Agent = new(AgentSourceName, "1.0.0");

    /// <summary>
    /// ActivitySource for MassTransit operations.
    /// </summary>
    public static readonly ActivitySource MassTransit = new(MassTransitSourceName, "1.0.0");
}

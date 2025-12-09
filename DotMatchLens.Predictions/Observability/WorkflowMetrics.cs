using System.Diagnostics.Metrics;

namespace DotMatchLens.Predictions.Observability;

/// <summary>
/// Metrics for workflow operations using OpenTelemetry.
/// </summary>
public sealed class WorkflowMetrics : IDisposable
{
    private readonly Meter _meter;

    // Counters
    private readonly Counter<long> _workflowsStarted;
    private readonly Counter<long> _workflowsCompleted;
    private readonly Counter<long> _workflowsFailed;
    private readonly Counter<long> _agentInvocations;
    private readonly Counter<long> _agentErrors;
    private readonly Counter<long> _predictionsGenerated;

    // Histograms
    private readonly Histogram<double> _workflowDuration;
    private readonly Histogram<double> _agentResponseTime;
    private readonly Histogram<double> _predictionConfidence;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowMetrics"/> class.
    /// </summary>
    public WorkflowMetrics()
    {
        _meter = new Meter("DotMatchLens.Predictions.Workflow", "1.0.0");

        // Initialize counters
        _workflowsStarted = _meter.CreateCounter<long>(
            "workflows.started",
            unit: "{workflow}",
            description: "Number of workflows started");

        _workflowsCompleted = _meter.CreateCounter<long>(
            "workflows.completed",
            unit: "{workflow}",
            description: "Number of workflows completed successfully");

        _workflowsFailed = _meter.CreateCounter<long>(
            "workflows.failed",
            unit: "{workflow}",
            description: "Number of workflows that failed");

        _agentInvocations = _meter.CreateCounter<long>(
            "agent.invocations",
            unit: "{invocation}",
            description: "Number of agent invocations");

        _agentErrors = _meter.CreateCounter<long>(
            "agent.errors",
            unit: "{error}",
            description: "Number of agent errors");

        _predictionsGenerated = _meter.CreateCounter<long>(
            "predictions.generated",
            unit: "{prediction}",
            description: "Number of predictions generated");

        // Initialize histograms
        _workflowDuration = _meter.CreateHistogram<double>(
            "workflow.duration",
            unit: "ms",
            description: "Duration of workflow execution in milliseconds");

        _agentResponseTime = _meter.CreateHistogram<double>(
            "agent.response_time",
            unit: "ms",
            description: "Agent response time in milliseconds");

        _predictionConfidence = _meter.CreateHistogram<double>(
            "prediction.confidence",
            unit: "1",
            description: "Prediction confidence score (0-1)");
    }

    /// <summary>
    /// Records a workflow start event.
    /// </summary>
    /// <param name="workflowType">Type of workflow (e.g., "match_prediction", "batch_prediction").</param>
    public void RecordWorkflowStarted(string workflowType)
    {
        _workflowsStarted.Add(1, new KeyValuePair<string, object?>("workflow.type", workflowType));
    }

    /// <summary>
    /// Records a workflow completion event.
    /// </summary>
    /// <param name="workflowType">Type of workflow.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public void RecordWorkflowCompleted(string workflowType, double durationMs)
    {
        var tags = new KeyValuePair<string, object?>("workflow.type", workflowType);
        _workflowsCompleted.Add(1, tags);
        _workflowDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Records a workflow failure event.
    /// </summary>
    /// <param name="workflowType">Type of workflow.</param>
    /// <param name="errorType">Type of error that occurred.</param>
    public void RecordWorkflowFailed(string workflowType, string errorType)
    {
        _workflowsFailed.Add(1,
            new KeyValuePair<string, object?>("workflow.type", workflowType),
            new KeyValuePair<string, object?>("error.type", errorType));
    }

    /// <summary>
    /// Records an agent invocation event.
    /// </summary>
    /// <param name="agentType">Type of agent (e.g., "ollama", "openai").</param>
    /// <param name="modelName">Name of the model used.</param>
    public void RecordAgentInvocation(string agentType, string modelName)
    {
        _agentInvocations.Add(1,
            new KeyValuePair<string, object?>("agent.type", agentType),
            new KeyValuePair<string, object?>("model.name", modelName));
    }

    /// <summary>
    /// Records an agent response event.
    /// </summary>
    /// <param name="agentType">Type of agent.</param>
    /// <param name="modelName">Name of the model used.</param>
    /// <param name="responseTimeMs">Response time in milliseconds.</param>
    public void RecordAgentResponse(string agentType, string modelName, double responseTimeMs)
    {
        var tags = new[]
        {
            new KeyValuePair<string, object?>("agent.type", agentType),
            new KeyValuePair<string, object?>("model.name", modelName)
        };
        _agentResponseTime.Record(responseTimeMs, tags);
    }

    /// <summary>
    /// Records an agent error event.
    /// </summary>
    /// <param name="agentType">Type of agent.</param>
    /// <param name="errorType">Type of error that occurred.</param>
    public void RecordAgentError(string agentType, string errorType)
    {
        _agentErrors.Add(1,
            new KeyValuePair<string, object?>("agent.type", agentType),
            new KeyValuePair<string, object?>("error.type", errorType));
    }

    /// <summary>
    /// Records a prediction generation event.
    /// </summary>
    /// <param name="matchId">ID of the match.</param>
    /// <param name="confidence">Confidence score of the prediction.</param>
    public void RecordPredictionGenerated(Guid matchId, float confidence)
    {
        var tags = new KeyValuePair<string, object?>("match.id", matchId.ToString());
        _predictionsGenerated.Add(1, tags);
        _predictionConfidence.Record(confidence, tags);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _meter.Dispose();
    }
}

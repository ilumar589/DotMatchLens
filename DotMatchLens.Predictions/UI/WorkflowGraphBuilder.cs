namespace DotMatchLens.Predictions.UI;

/// <summary>
/// DTO for workflow graph node.
/// </summary>
public sealed record WorkflowNodeDto(
    string Id,
    string Name,
    string Type,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    Dictionary<string, object>? Metadata);

/// <summary>
/// DTO for workflow graph edge.
/// </summary>
public sealed record WorkflowEdgeDto(
    string Id,
    string Source,
    string Target,
    string Label);

/// <summary>
/// DTO for workflow graph.
/// </summary>
public sealed record WorkflowGraphDto(
    string WorkflowId,
    string WorkflowType,
    string Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    IReadOnlyList<WorkflowNodeDto> Nodes,
    IReadOnlyList<WorkflowEdgeDto> Edges,
    Dictionary<string, object>? Metadata);

/// <summary>
/// DTO for workflow event.
/// </summary>
public sealed record WorkflowEventDto(
    string EventId,
    string WorkflowId,
    string EventType,
    string NodeId,
    DateTime Timestamp,
    Dictionary<string, object>? Data);

/// <summary>
/// Builds workflow visualization graphs from workflow execution data.
/// </summary>
public sealed class WorkflowGraphBuilder
{
    /// <summary>
    /// Builds a workflow graph for match prediction workflow.
    /// </summary>
    /// <param name="workflowId">Workflow correlation ID.</param>
    /// <param name="matchId">Match ID being predicted.</param>
    /// <param name="status">Current workflow status.</param>
    /// <param name="startedAt">When the workflow started.</param>
    /// <param name="completedAt">When the workflow completed (if applicable).</param>
    /// <param name="events">List of workflow events.</param>
    /// <returns>Workflow graph DTO.</returns>
    public static WorkflowGraphDto BuildMatchPredictionGraph(
        string workflowId,
        Guid matchId,
        string status,
        DateTime startedAt,
        DateTime? completedAt,
        IReadOnlyList<WorkflowEventDto> events)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        ArgumentNullException.ThrowIfNull(events);

        var nodes = new List<WorkflowNodeDto>
        {
            new("start", "Start", "start", "completed", startedAt, startedAt, null),
            new("receive_request", "Receive Request", "consumer", GetNodeStatus(events, "receive_request"), 
                GetNodeStartTime(events, "receive_request"), GetNodeEndTime(events, "receive_request"), null),
            new("fetch_match", "Fetch Match Data", "step", GetNodeStatus(events, "fetch_match"), 
                GetNodeStartTime(events, "fetch_match"), GetNodeEndTime(events, "fetch_match"), null),
            new("invoke_agent", "Invoke AI Agent", "agent", GetNodeStatus(events, "invoke_agent"), 
                GetNodeStartTime(events, "invoke_agent"), GetNodeEndTime(events, "invoke_agent"), null),
            new("save_prediction", "Save Prediction", "step", GetNodeStatus(events, "save_prediction"), 
                GetNodeStartTime(events, "save_prediction"), GetNodeEndTime(events, "save_prediction"), null),
            new("publish_result", "Publish Result", "publisher", GetNodeStatus(events, "publish_result"), 
                GetNodeStartTime(events, "publish_result"), GetNodeEndTime(events, "publish_result"), null),
            new("end", "End", "end", status == "completed" ? "completed" : "pending", completedAt, completedAt, null)
        };

        var edges = new List<WorkflowEdgeDto>
        {
            new("e1", "start", "receive_request", "trigger"),
            new("e2", "receive_request", "fetch_match", "next"),
            new("e3", "fetch_match", "invoke_agent", "next"),
            new("e4", "invoke_agent", "save_prediction", "next"),
            new("e5", "save_prediction", "publish_result", "next"),
            new("e6", "publish_result", "end", "complete")
        };

        var metadata = new Dictionary<string, object>
        {
            { "matchId", matchId.ToString() },
            { "eventCount", events.Count }
        };

        return new WorkflowGraphDto(
            workflowId,
            "match_prediction",
            status,
            startedAt,
            completedAt,
            nodes,
            edges,
            metadata);
    }

    /// <summary>
    /// Builds a workflow graph for batch prediction workflow.
    /// </summary>
    /// <param name="workflowId">Workflow correlation ID.</param>
    /// <param name="batchSize">Number of items in the batch.</param>
    /// <param name="status">Current workflow status.</param>
    /// <param name="startedAt">When the workflow started.</param>
    /// <param name="completedAt">When the workflow completed (if applicable).</param>
    /// <param name="completedCount">Number of completed items.</param>
    /// <returns>Workflow graph DTO.</returns>
    public static WorkflowGraphDto BuildBatchPredictionGraph(
        string workflowId,
        int batchSize,
        string status,
        DateTime startedAt,
        DateTime? completedAt,
        int completedCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);

        var nodes = new List<WorkflowNodeDto>
        {
            new("start", "Start", "start", "completed", startedAt, startedAt, null),
            new("receive_batch", "Receive Batch", "consumer", "completed", startedAt, startedAt, null),
            new("process_batch", "Process Batch", "parallel", 
                completedCount == batchSize ? "completed" : "running", 
                startedAt, completedCount == batchSize ? completedAt : null, 
                new Dictionary<string, object>
                {
                    { "batchSize", batchSize },
                    { "completedCount", completedCount }
                }),
            new("aggregate_results", "Aggregate Results", "step", 
                status == "completed" ? "completed" : "pending", 
                completedAt, completedAt, null),
            new("end", "End", "end", status == "completed" ? "completed" : "pending", completedAt, completedAt, null)
        };

        var edges = new List<WorkflowEdgeDto>
        {
            new("e1", "start", "receive_batch", "trigger"),
            new("e2", "receive_batch", "process_batch", "distribute"),
            new("e3", "process_batch", "aggregate_results", "collect"),
            new("e4", "aggregate_results", "end", "complete")
        };

        var metadata = new Dictionary<string, object>
        {
            { "batchSize", batchSize },
            { "completedCount", completedCount },
            { "progress", batchSize > 0 ? (double)completedCount / batchSize : 0.0 }
        };

        return new WorkflowGraphDto(
            workflowId,
            "batch_prediction",
            status,
            startedAt,
            completedAt,
            nodes,
            edges,
            metadata);
    }

    private static string GetNodeStatus(IReadOnlyList<WorkflowEventDto> events, string nodeId)
    {
        var nodeEvents = events.Where(e => e.NodeId == nodeId).ToList();
        if (nodeEvents.Count == 0)
            return "pending";

        if (nodeEvents.Any(e => e.EventType == "failed"))
            return "failed";

        if (nodeEvents.Any(e => e.EventType == "completed"))
            return "completed";

        if (nodeEvents.Any(e => e.EventType == "started"))
            return "running";

        return "pending";
    }

    private static DateTime? GetNodeStartTime(IReadOnlyList<WorkflowEventDto> events, string nodeId)
    {
        return events
            .Where(e => e.NodeId == nodeId && e.EventType == "started")
            .OrderBy(e => e.Timestamp)
            .FirstOrDefault()
            ?.Timestamp;
    }

    private static DateTime? GetNodeEndTime(IReadOnlyList<WorkflowEventDto> events, string nodeId)
    {
        return events
            .Where(e => e.NodeId == nodeId && (e.EventType == "completed" || e.EventType == "failed"))
            .OrderBy(e => e.Timestamp)
            .FirstOrDefault()
            ?.Timestamp;
    }
}

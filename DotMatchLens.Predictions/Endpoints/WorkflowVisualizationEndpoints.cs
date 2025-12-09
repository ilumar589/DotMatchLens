using System.Globalization;
using System.Text;
using System.Text.Json;
using DotMatchLens.Predictions.Configuration;
using DotMatchLens.Predictions.UI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace DotMatchLens.Predictions.Endpoints;

/// <summary>
/// Workflow visualization API endpoints for AGUI integration.
/// </summary>
public static class WorkflowVisualizationEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Maps workflow visualization endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapWorkflowVisualizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/workflows")
            .WithTags("Workflow Visualization");

        group.MapGet("/graph/{workflowId:guid}", GetWorkflowGraph)
            .WithName("GetWorkflowGraph")
            .WithDescription("Get workflow execution graph for visualization");

        group.MapGet("/events/{workflowId:guid}", GetWorkflowEvents)
            .WithName("GetWorkflowEvents")
            .WithDescription("Get workflow events");

        group.MapGet("/events/{workflowId:guid}/stream", StreamWorkflowEvents)
            .WithName("StreamWorkflowEvents")
            .WithDescription("Stream workflow events via Server-Sent Events (SSE)");

        group.MapGet("/active", GetActiveWorkflows)
            .WithName("GetActiveWorkflows")
            .WithDescription("Get list of active workflows");

        return endpoints;
    }

    private static Ok<WorkflowGraphDto> GetWorkflowGraph(
        Guid workflowId,
        IOptions<WorkflowOptions> options)
    {
        // In a real implementation, you would fetch this data from a persistence store
        // For now, we'll return a sample graph
        var events = new List<WorkflowEventDto>
        {
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "started", "receive_request", DateTime.UtcNow.AddMinutes(-1), null),
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "completed", "receive_request", DateTime.UtcNow.AddMinutes(-1).AddSeconds(5), null),
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "started", "fetch_match", DateTime.UtcNow.AddMinutes(-1).AddSeconds(5), null),
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "completed", "fetch_match", DateTime.UtcNow.AddMinutes(-1).AddSeconds(10), null),
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "started", "invoke_agent", DateTime.UtcNow.AddMinutes(-1).AddSeconds(10), null),
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "completed", "invoke_agent", DateTime.UtcNow.AddMinutes(-1).AddSeconds(30), null),
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "started", "save_prediction", DateTime.UtcNow.AddMinutes(-1).AddSeconds(30), null),
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "completed", "save_prediction", DateTime.UtcNow.AddMinutes(-1).AddSeconds(32), null),
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "started", "publish_result", DateTime.UtcNow.AddMinutes(-1).AddSeconds(32), null),
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "completed", "publish_result", DateTime.UtcNow.AddMinutes(-1).AddSeconds(33), null)
        };

        var graph = WorkflowGraphBuilder.BuildMatchPredictionGraph(
            workflowId.ToString(),
            Guid.NewGuid(),
            "completed",
            DateTime.UtcNow.AddMinutes(-1),
            DateTime.UtcNow.AddSeconds(-27),
            events);

        return TypedResults.Ok(graph);
    }

    private static Ok<IReadOnlyList<WorkflowEventDto>> GetWorkflowEvents(
        Guid workflowId,
        IOptions<WorkflowOptions> options)
    {
        // In a real implementation, fetch from event store
        var events = new List<WorkflowEventDto>
        {
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "started", "workflow", DateTime.UtcNow.AddMinutes(-1), 
                new Dictionary<string, object> { { "workflowType", "match_prediction" } }),
            new(Guid.NewGuid().ToString(), workflowId.ToString(), "completed", "workflow", DateTime.UtcNow.AddSeconds(-27), 
                new Dictionary<string, object> { { "status", "success" } })
        };

        return TypedResults.Ok<IReadOnlyList<WorkflowEventDto>>(events);
    }

    private static async Task StreamWorkflowEvents(
        Guid workflowId,
        HttpContext context,
        IOptions<WorkflowOptions> options,
        CancellationToken cancellationToken)
    {
        var workflowOptions = options.Value;

        if (!workflowOptions.EnableSseEvents)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Server-Sent Events are disabled", cancellationToken);
            return;
        }

        context.Response.ContentType = "text/event-stream";
        context.Response.Headers.CacheControl = "no-cache";
        context.Response.Headers.Connection = "keep-alive";

        // Send initial connection event
        await SendSseEventAsync(context.Response, "connected", new
        {
            workflowId = workflowId.ToString(),
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        // In a real implementation, you would subscribe to workflow events
        // and stream them as they occur. For now, we'll send a few sample events
        var events = new[]
        {
            new { eventType = "workflow_started", nodeId = "start", timestamp = DateTime.UtcNow },
            new { eventType = "step_completed", nodeId = "receive_request", timestamp = DateTime.UtcNow.AddSeconds(1) },
            new { eventType = "step_started", nodeId = "fetch_match", timestamp = DateTime.UtcNow.AddSeconds(2) }
        };

        foreach (var evt in events)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await Task.Delay(1000, cancellationToken);
            await SendSseEventAsync(context.Response, "workflow_event", evt, cancellationToken);
        }

        // Keep connection alive
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(5000, cancellationToken);
            await SendSseEventAsync(context.Response, "heartbeat", new
            {
                timestamp = DateTime.UtcNow
            }, cancellationToken);
        }
    }

    private static Ok<IReadOnlyList<ActiveWorkflowDto>> GetActiveWorkflows(
        IOptions<WorkflowOptions> options)
    {
        // In a real implementation, fetch from workflow state store
        var workflows = new List<ActiveWorkflowDto>
        {
            new(Guid.NewGuid().ToString(), "match_prediction", "running", DateTime.UtcNow.AddMinutes(-5)),
            new(Guid.NewGuid().ToString(), "batch_prediction", "running", DateTime.UtcNow.AddMinutes(-2))
        };

        return TypedResults.Ok<IReadOnlyList<ActiveWorkflowDto>>(workflows);
    }

    private static async Task SendSseEventAsync(
        HttpResponse response,
        string eventType,
        object data,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);

        var sseMessage = new StringBuilder();
        sseMessage.AppendLine(CultureInfo.InvariantCulture, $"event: {eventType}");
        sseMessage.AppendLine(CultureInfo.InvariantCulture, $"data: {json}");
        sseMessage.AppendLine();

        await response.WriteAsync(sseMessage.ToString(), cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}

/// <summary>
/// DTO for active workflow.
/// </summary>
public sealed record ActiveWorkflowDto(
    string WorkflowId,
    string WorkflowType,
    string Status,
    DateTime StartedAt);

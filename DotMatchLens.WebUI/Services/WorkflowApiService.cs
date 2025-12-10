namespace DotMatchLens.WebUI.Services;

/// <summary>
/// HTTP client service for Workflow API endpoints.
/// </summary>
public sealed class WorkflowApiService
{
    private readonly HttpClient _httpClient;

    public WorkflowApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WorkflowTriggerResponse?> TriggerMatchPredictionWorkflowAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(new Uri($"/api/predictions/workflow/match/{matchId}", UriKind.Relative), null, cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<WorkflowTriggerResponse>(cancellationToken);
        }
        
        return null;
    }

    public async Task<BatchWorkflowTriggerResponse?> TriggerBatchPredictionWorkflowAsync(IReadOnlyList<Guid> matchIds, CancellationToken cancellationToken = default)
    {
        var request = new { MatchIds = matchIds };
        var response = await _httpClient.PostAsJsonAsync(new Uri("/api/predictions/workflow/batch", UriKind.Relative), request, cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<BatchWorkflowTriggerResponse>(cancellationToken);
        }
        
        return null;
    }

    public async Task<WorkflowGraphDto?> GetWorkflowGraphAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<WorkflowGraphDto>($"/api/workflows/graph/{workflowId}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<ActiveWorkflowDto>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<List<ActiveWorkflowDto>>("/api/workflows/active", cancellationToken);
        return response ?? [];
    }
}

// DTOs for workflow responses
public sealed record WorkflowTriggerResponse(Guid CorrelationId, Guid MatchId, string Message);
public sealed record BatchWorkflowTriggerResponse(IReadOnlyList<Guid> CorrelationIds, int Count, string Message);
public sealed record ActiveWorkflowDto(string WorkflowId, string WorkflowType, string Status, DateTime StartedAt);
public sealed record WorkflowGraphDto(string WorkflowId, string WorkflowType, string Status, DateTime StartedAt, DateTime? CompletedAt, IReadOnlyList<WorkflowNodeDto> Nodes, IReadOnlyList<WorkflowEdgeDto> Edges);
public sealed record WorkflowNodeDto(string Id, string Label, string Type, string Status);
public sealed record WorkflowEdgeDto(string From, string To, string? Label);
public sealed record WorkflowEventDto(string EventId, string WorkflowId, string EventType, string NodeId, DateTime Timestamp, Dictionary<string, object>? Data);

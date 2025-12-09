using DotMatchLens.Data.Context;
using DotMatchLens.Predictions.Agents;
using DotMatchLens.Predictions.Models;
using DotMatchLens.Predictions.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DotMatchLens.Predictions.Endpoints;

/// <summary>
/// Prediction API endpoints registration.
/// </summary>
public static class PredictionEndpoints
{
    /// <summary>
    /// Maps all prediction-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapPredictionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/predictions")
            .WithTags("Predictions");

        group.MapPost("/generate", GeneratePredictionAsync)
            .WithName("GeneratePrediction")
            .WithDescription("Generate a prediction for a match using AI");

        group.MapGet("/match/{matchId:guid}", GetPredictionsForMatchAsync)
            .WithName("GetPredictionsForMatch")
            .WithDescription("Get all predictions for a specific match");

        group.MapPost("/query", QueryAgentAsync)
            .WithName("QueryAgent")
            .WithDescription("Query the AI agent with a custom question using Microsoft Agent Framework");

        // Workflow-based prediction endpoints
        group.MapPost("/workflow/match/{matchId:guid}", TriggerMatchPredictionWorkflowAsync)
            .WithName("TriggerMatchPredictionWorkflow")
            .WithDescription("Trigger a match prediction using workflow orchestration via MassTransit");

        group.MapPost("/workflow/batch", TriggerBatchPredictionWorkflowAsync)
            .WithName("TriggerBatchPredictionWorkflow")
            .WithDescription("Trigger batch predictions for multiple matches");

        return endpoints;
    }

    private static async Task<Results<Created<PredictionDto>, NotFound<ErrorResponse>>> GeneratePredictionAsync(
        GeneratePredictionRequest request,
        PredictionService service,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prediction = await service.GeneratePredictionAsync(
                request.MatchId,
                request.AdditionalContext,
                cancellationToken)
                ;
            return TypedResults.Created($"/api/predictions/match/{prediction.MatchId}", prediction);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.NotFound(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<Ok<IReadOnlyList<PredictionDto>>> GetPredictionsForMatchAsync(
        Guid matchId,
        PredictionService service,
        CancellationToken cancellationToken = default)
    {
        var predictions = await service.GetPredictionsForMatchAsync(matchId, cancellationToken)
            ;
        return TypedResults.Ok(predictions);
    }

    private static async Task<Ok<AgentResponse>> QueryAgentAsync(
        QueryAgentRequest request,
        FootballAgentService agentService,
        FootballDbContext context,
        CancellationToken cancellationToken = default)
    {
        // Build match context if MatchId is provided
        string? matchContext = null;
        if (request.MatchId.HasValue)
        {
            var match = await context.Matches
                .AsNoTracking()
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == request.MatchId.Value, cancellationToken)
                ;

            if (match is not null)
            {
                matchContext = $"Match: {match.HomeTeam?.Name ?? "Unknown"} vs {match.AwayTeam?.Name ?? "Unknown"} on {match.MatchDate:yyyy-MM-dd}";
            }
        }

        // Use FootballAgentService with Microsoft Agent Framework instead of custom implementation
        var response = await agentService.QueryAsync(request.Query, matchContext, cancellationToken)
            ;
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Accepted<WorkflowTriggerResponse>, NotFound<ErrorResponse>>> TriggerMatchPredictionWorkflowAsync(
        Guid matchId,
        MassTransit.IPublishEndpoint publishEndpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var correlationId = Guid.NewGuid();

            await publishEndpoint.Publish(new DotMatchLens.Core.Contracts.MatchPredictionRequested
            {
                MatchId = matchId,
                CorrelationId = correlationId,
                AdditionalContext = null
            }, cancellationToken);

            return TypedResults.Accepted(
                $"/api/predictions/{correlationId}/status",
                new WorkflowTriggerResponse(correlationId, matchId, "Match prediction workflow triggered"));
        }
        catch (Exception ex)
        {
            return TypedResults.NotFound(new ErrorResponse($"Failed to trigger workflow: {ex.Message}"));
        }
    }

    private static async Task<Results<Accepted<BatchWorkflowTriggerResponse>, BadRequest<ErrorResponse>>> TriggerBatchPredictionWorkflowAsync(
        BatchPredictionRequest request,
        MassTransit.IPublishEndpoint publishEndpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var correlationIds = new List<Guid>();

            foreach (var matchId in request.MatchIds)
            {
                var correlationId = Guid.NewGuid();
                correlationIds.Add(correlationId);

                await publishEndpoint.Publish(new DotMatchLens.Core.Contracts.MatchPredictionRequested
                {
                    MatchId = matchId,
                    CorrelationId = correlationId,
                    AdditionalContext = null
                }, cancellationToken);
            }

            return TypedResults.Accepted(
                "/api/predictions/batch/status",
                new BatchWorkflowTriggerResponse(correlationIds.AsReadOnly(), request.MatchIds.Count, "Batch prediction workflows triggered"));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new ErrorResponse($"Failed to trigger batch workflow: {ex.Message}"));
        }
    }
}

/// <summary>
/// Error response model.
/// </summary>
public sealed record ErrorResponse(string Error);

/// <summary>
/// Workflow trigger response.
/// </summary>
public sealed record WorkflowTriggerResponse(Guid CorrelationId, Guid MatchId, string Message);

/// <summary>
/// Batch workflow trigger response.
/// </summary>
public sealed record BatchWorkflowTriggerResponse(IReadOnlyList<Guid> CorrelationIds, int Count, string Message);

/// <summary>
/// Batch prediction request.
/// </summary>
public sealed record BatchPredictionRequest(IReadOnlyList<Guid> MatchIds);

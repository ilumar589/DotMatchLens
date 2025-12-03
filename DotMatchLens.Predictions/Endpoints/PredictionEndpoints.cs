using DotMatchLens.Predictions.Agents;
using DotMatchLens.Predictions.Models;
using DotMatchLens.Predictions.Services;
using Microsoft.AspNetCore.Http.HttpResults;

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
                .ConfigureAwait(false);
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
            .ConfigureAwait(false);
        return TypedResults.Ok(predictions);
    }

    private static async Task<Ok<AgentResponse>> QueryAgentAsync(
        QueryAgentRequest request,
        FootballAgentService agentService,
        CancellationToken cancellationToken = default)
    {
        // Use FootballAgentService with Microsoft Agent Framework instead of custom implementation
        var response = await agentService.QueryAsync(request.Query, cancellationToken)
            .ConfigureAwait(false);
        return TypedResults.Ok(response);
    }
}

/// <summary>
/// Error response model.
/// </summary>
public sealed record ErrorResponse(string Error);

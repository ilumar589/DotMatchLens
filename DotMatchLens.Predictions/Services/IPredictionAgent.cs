using System.Collections.Immutable;

namespace DotMatchLens.Predictions.Services;

/// <summary>
/// Interface for the prediction AI agent.
/// </summary>
public interface IPredictionAgent
{
    /// <summary>
    /// Generate a prediction for a match.
    /// </summary>
    Task<AgentPredictionResult> GeneratePredictionAsync(
        string homeTeamName,
        string awayTeamName,
        DateTime matchDate,
        string? additionalContext = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the agent with a custom question.
    /// </summary>
    Task<AgentQueryResult> QueryAsync(
        string query,
        string? context = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from the AI agent prediction.
/// </summary>
public sealed record AgentPredictionResult(
    float HomeWinProbability,
    float DrawProbability,
    float AwayWinProbability,
    int? PredictedHomeScore,
    int? PredictedAwayScore,
    string? Reasoning,
    float Confidence,
    string? ModelVersion,
    ImmutableArray<float>? Embedding);

/// <summary>
/// Result from a custom query to the agent.
/// </summary>
public sealed record AgentQueryResult(
    string Response,
    string? ModelVersion,
    float? Confidence);

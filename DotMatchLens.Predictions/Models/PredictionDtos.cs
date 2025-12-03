namespace DotMatchLens.Predictions.Models;

/// <summary>
/// Readonly record struct for prediction data transfer.
/// </summary>
public readonly record struct PredictionDto(
    Guid Id,
    Guid MatchId,
    string HomeTeamName,
    string AwayTeamName,
    float HomeWinProbability,
    float DrawProbability,
    float AwayWinProbability,
    int? PredictedHomeScore,
    int? PredictedAwayScore,
    string? Reasoning,
    float Confidence,
    string? ModelVersion,
    DateTime PredictedAt);

/// <summary>
/// Request model for generating a prediction.
/// </summary>
public sealed record GeneratePredictionRequest(
    Guid MatchId,
    string? AdditionalContext = null);

/// <summary>
/// Request model for querying the AI agent.
/// </summary>
public sealed record QueryAgentRequest(
    string Query,
    Guid? MatchId = null);

/// <summary>
/// Response model from the AI agent.
/// </summary>
public sealed record AgentResponse(
    string Response,
    string? ModelVersion = null,
    float? Confidence = null);

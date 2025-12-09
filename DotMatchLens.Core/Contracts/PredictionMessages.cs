namespace DotMatchLens.Core.Contracts;

/// <summary>
/// Message requesting a prediction for a match.
/// </summary>
public sealed record MatchPredictionRequested
{
    public required Guid MatchId { get; init; }
    public required Guid CorrelationId { get; init; }
    public string? AdditionalContext { get; init; }
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Message indicating match prediction has completed.
/// </summary>
public sealed record MatchPredictionCompleted
{
    public required Guid MatchId { get; init; }
    public required Guid PredictionId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public float? Confidence { get; init; }
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

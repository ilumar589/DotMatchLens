namespace DotMatchLens.Data.Entities;

/// <summary>
/// Represents an AI-generated prediction for a match.
/// </summary>
public sealed class MatchPrediction
{
    public Guid Id { get; init; }
    public Guid MatchId { get; init; }
    public float HomeWinProbability { get; init; }
    public float DrawProbability { get; init; }
    public float AwayWinProbability { get; init; }
    public int? PredictedHomeScore { get; init; }
    public int? PredictedAwayScore { get; init; }
    public string? Reasoning { get; init; }
    public string? ModelVersion { get; init; }
    public float Confidence { get; init; }
    public DateTime PredictedAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Vector embedding of the match context for similarity search.
    /// </summary>
    public Vector? ContextEmbedding { get; init; }

    // Navigation properties
    public Match? Match { get; init; }
}

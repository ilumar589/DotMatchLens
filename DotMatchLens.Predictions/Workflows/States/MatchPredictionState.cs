namespace DotMatchLens.Predictions.Workflows.States;

/// <summary>
/// State for the match prediction workflow.
/// Holds all data needed during the prediction process.
/// </summary>
public sealed class MatchPredictionState
{
    public Guid MatchId { get; set; }
    public Guid HomeTeamId { get; set; }
    public string HomeTeamName { get; set; } = string.Empty;
    public Guid AwayTeamId { get; set; }
    public string AwayTeamName { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    
    // Gathered data (use Collection instead of List for CA1002 and make init-only for CA2227)
    public ICollection<Tools.TeamInfo> HomeTeamData { get; init; } = [];
    public ICollection<Tools.TeamInfo> AwayTeamData { get; init; } = [];
    public ICollection<Tools.SimilarMatchInfo> SimilarMatches { get; init; } = [];
    
    // Prediction results
    public float HomeWinProbability { get; set; }
    public float DrawProbability { get; set; }
    public float AwayWinProbability { get; set; }
    public int? PredictedHomeScore { get; set; }
    public int? PredictedAwayScore { get; set; }
    public string? Reasoning { get; set; }
    public float Confidence { get; set; }
    public Guid? PredictionId { get; set; }
    
    // Error tracking
    public string? ErrorMessage { get; set; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
}

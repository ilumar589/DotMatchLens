namespace DotMatchLens.Data.Entities;

/// <summary>
/// Represents a football match.
/// </summary>
public sealed class Match
{
    public Guid Id { get; init; }
    public Guid HomeTeamId { get; init; }
    public Guid AwayTeamId { get; init; }
    public DateTime MatchDate { get; init; }
    public string? Stadium { get; init; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Team? HomeTeam { get; init; }
    public Team? AwayTeam { get; init; }
    public ICollection<MatchEvent> Events { get; init; } = [];
    public ICollection<MatchPrediction> Predictions { get; init; } = [];
}

/// <summary>
/// Match status enumeration.
/// </summary>
public enum MatchStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Postponed = 3,
    Cancelled = 4
}

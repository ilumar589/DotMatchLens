namespace DotMatchLens.Data.Entities;

/// <summary>
/// Represents a football season within a competition.
/// </summary>
public sealed class Season
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// External ID from football-data.org API.
    /// </summary>
    public int ExternalId { get; init; }
    
    /// <summary>
    /// Reference to the parent competition.
    /// </summary>
    public Guid CompetitionId { get; init; }
    
    /// <summary>
    /// Season start date.
    /// </summary>
    public DateOnly StartDate { get; init; }
    
    /// <summary>
    /// Season end date.
    /// </summary>
    public DateOnly EndDate { get; init; }
    
    /// <summary>
    /// Current matchday (for ongoing seasons).
    /// </summary>
    public int? CurrentMatchday { get; set; }
    
    /// <summary>
    /// Winner team external ID (from football-data.org).
    /// </summary>
    public int? WinnerExternalId { get; set; }
    
    /// <summary>
    /// Winner team name.
    /// </summary>
    public string? WinnerName { get; set; }
    
    /// <summary>
    /// Competition stages in this season (e.g., "REGULAR_SEASON", "PLAYOFFS").
    /// </summary>
    public ImmutableArray<string>? Stages { get; init; }
    
    /// <summary>
    /// Raw JSON response for the season stored in JSONB column.
    /// </summary>
    public string? RawJson { get; set; }
    
    /// <summary>
    /// Vector embedding for semantic search.
    /// </summary>
    public Vector? Embedding { get; set; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Competition? Competition { get; init; }
}

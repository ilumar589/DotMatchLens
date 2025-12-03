namespace DotMatchLens.Data.Entities;

/// <summary>
/// Represents a football competition (league/tournament).
/// </summary>
public sealed class Competition
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// External ID from football-data.org API.
    /// </summary>
    public int ExternalId { get; init; }
    
    public required string Name { get; init; }
    
    /// <summary>
    /// Competition code (e.g., "PL" for Premier League).
    /// </summary>
    public required string Code { get; init; }
    
    /// <summary>
    /// Competition type (e.g., "LEAGUE", "CUP").
    /// </summary>
    public string? Type { get; init; }
    
    /// <summary>
    /// URL to the competition emblem.
    /// </summary>
    public string? Emblem { get; init; }
    
    /// <summary>
    /// Area/country name.
    /// </summary>
    public string? AreaName { get; init; }
    
    /// <summary>
    /// Area/country code.
    /// </summary>
    public string? AreaCode { get; init; }
    
    /// <summary>
    /// Area/country flag URL.
    /// </summary>
    public string? AreaFlag { get; init; }
    
    /// <summary>
    /// Raw JSON response from the API stored in JSONB column.
    /// </summary>
    public string? RawJson { get; set; }
    
    /// <summary>
    /// Vector embedding for semantic search.
    /// </summary>
    public Vector? Embedding { get; set; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? SyncedAt { get; set; }

    // Navigation properties
    public ICollection<Season> Seasons { get; init; } = [];
}

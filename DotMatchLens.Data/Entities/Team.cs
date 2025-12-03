namespace DotMatchLens.Data.Entities;

/// <summary>
/// Represents a football team.
/// </summary>
public sealed class Team
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// External ID from football-data.org API.
    /// </summary>
    public int? ExternalId { get; init; }
    
    public required string Name { get; init; }
    
    /// <summary>
    /// Short name of the team.
    /// </summary>
    public string? ShortName { get; init; }
    
    /// <summary>
    /// Three-letter abbreviation.
    /// </summary>
    public string? Tla { get; init; }
    
    public string? Country { get; init; }
    public string? League { get; init; }
    
    /// <summary>
    /// URL to the team crest/logo.
    /// </summary>
    public string? Crest { get; init; }
    
    /// <summary>
    /// Team address.
    /// </summary>
    public string? Address { get; init; }
    
    /// <summary>
    /// Team website URL.
    /// </summary>
    public string? Website { get; init; }
    
    /// <summary>
    /// Year the team was founded.
    /// </summary>
    public int? Founded { get; init; }
    
    /// <summary>
    /// Club colors.
    /// </summary>
    public string? ClubColors { get; init; }
    
    /// <summary>
    /// Home venue/stadium name.
    /// </summary>
    public string? Venue { get; init; }
    
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

    // Navigation properties
    public ICollection<Player> Players { get; init; } = [];
    public ICollection<Match> HomeMatches { get; init; } = [];
    public ICollection<Match> AwayMatches { get; init; } = [];
}

namespace DotMatchLens.Data.Entities;

/// <summary>
/// Represents a football team.
/// </summary>
public sealed class Team
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Country { get; init; }
    public string? League { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Player> Players { get; init; } = [];
    public ICollection<Match> HomeMatches { get; init; } = [];
    public ICollection<Match> AwayMatches { get; init; } = [];
}

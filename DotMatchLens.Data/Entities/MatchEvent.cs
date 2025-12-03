namespace DotMatchLens.Data.Entities;

/// <summary>
/// Represents an event that occurred during a match (goal, card, substitution, etc.).
/// </summary>
public sealed class MatchEvent
{
    public Guid Id { get; init; }
    public Guid MatchId { get; init; }
    public Guid? PlayerId { get; init; }
    public required string EventType { get; init; }
    public int Minute { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Navigation properties
    public Match? Match { get; init; }
    public Player? Player { get; init; }
}

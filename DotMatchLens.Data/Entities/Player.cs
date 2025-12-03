namespace DotMatchLens.Data.Entities;

/// <summary>
/// Represents a football player.
/// </summary>
public sealed class Player
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Position { get; init; }
    public int? JerseyNumber { get; set; }
    public DateOnly? DateOfBirth { get; init; }
    public Guid? TeamId { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Team? Team { get; set; }
}

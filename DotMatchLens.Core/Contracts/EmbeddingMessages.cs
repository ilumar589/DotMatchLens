namespace DotMatchLens.Core.Contracts;

/// <summary>
/// Message requesting embedding generation for an entity.
/// </summary>
public sealed record EmbeddingGenerationRequested
{
    public required string EntityType { get; init; } // "Team", "Competition", "Season"
    public required Guid EntityId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required string Text { get; init; }
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Message indicating embedding generation has completed.
/// </summary>
public sealed record EmbeddingGenerationCompleted
{
    public required string EntityType { get; init; }
    public required Guid EntityId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int? Dimensions { get; init; }
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

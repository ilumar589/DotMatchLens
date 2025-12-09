namespace DotMatchLens.Core.Contracts;

/// <summary>
/// Message requesting synchronization of competition data from external API.
/// </summary>
public sealed record CompetitionSyncRequested
{
    public required string CompetitionCode { get; init; }
    public required Guid CorrelationId { get; init; }
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Message indicating competition synchronization has completed.
/// </summary>
public sealed record CompetitionSyncCompleted
{
    public required string CompetitionCode { get; init; }
    public required Guid CorrelationId { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int SeasonsProcessed { get; init; }
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Message indicating team data has been ingested.
/// </summary>
public sealed record TeamDataIngested
{
    public required Guid TeamId { get; init; }
    public required string TeamName { get; init; }
    public string? Country { get; init; }
    public DateTime IngestedAt { get; init; } = DateTime.UtcNow;
}

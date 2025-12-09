using System.Collections.Immutable;

namespace DotMatchLens.Core.Services;

/// <summary>
/// Service for generating vector embeddings for semantic search.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates an embedding for the given text.
    /// </summary>
    /// <param name="text">The text to generate an embedding for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An immutable array of floats representing the embedding, or null if generation failed.</returns>
    Task<ImmutableArray<float>?> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an embedding for a competition description.
    /// </summary>
    Task<ImmutableArray<float>?> GenerateCompetitionEmbeddingAsync(
        string competitionName,
        string? areaName,
        string? type,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an embedding for a season summary.
    /// </summary>
    Task<ImmutableArray<float>?> GenerateSeasonEmbeddingAsync(
        string competitionName,
        DateOnly startDate,
        DateOnly endDate,
        string? winnerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an embedding for a team profile.
    /// </summary>
    Task<ImmutableArray<float>?> GenerateTeamEmbeddingAsync(
        string teamName,
        string? venue,
        string? clubColors,
        int? founded,
        string? country,
        CancellationToken cancellationToken = default);
}

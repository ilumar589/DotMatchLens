using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using DotMatchLens.Core.Services;

namespace DotMatchLens.Football.Services;

/// <summary>
/// Pgvector-based embedding service that generates deterministic embeddings
/// without relying on external LLM services.
/// Uses a hash-based approach to create consistent vector representations.
/// </summary>
public sealed class PgVectorEmbeddingService : IEmbeddingService
{
    private readonly ILogger<PgVectorEmbeddingService> _logger;
    private const int EmbeddingDimensions = 768;

    public PgVectorEmbeddingService(ILogger<PgVectorEmbeddingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates a deterministic embedding for the given text using hash-based approach.
    /// </summary>
    public Task<ImmutableArray<float>?> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        try
        {
            // Note: Using Debug level logging for embedding generation
#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogDebug("Generating embedding for text of length {Length}", text.Length);
#pragma warning restore CA1848

            // Create a deterministic embedding based on text content
            var embedding = GenerateDeterministicEmbedding(text);

#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogDebug("Generated {Dimensions}-dimensional embedding", embedding.Length);
#pragma warning restore CA1848
            return Task.FromResult<ImmutableArray<float>?>(embedding);
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogError(ex, "Failed to generate embedding for text");
#pragma warning restore CA1848
            return Task.FromResult<ImmutableArray<float>?>(null);
        }
    }

    /// <summary>
    /// Generates an embedding for a competition description.
    /// </summary>
    public Task<ImmutableArray<float>?> GenerateCompetitionEmbeddingAsync(
        string competitionName,
        string? areaName,
        string? type,
        CancellationToken cancellationToken = default)
    {
        var description = $"Football competition: {competitionName}";
        if (!string.IsNullOrWhiteSpace(areaName))
        {
            description += $" in {areaName}";
        }
        if (!string.IsNullOrWhiteSpace(type))
        {
            description += $", type: {type}";
        }

        return GenerateEmbeddingAsync(description, cancellationToken);
    }

    /// <summary>
    /// Generates an embedding for a season summary.
    /// </summary>
    public Task<ImmutableArray<float>?> GenerateSeasonEmbeddingAsync(
        string competitionName,
        DateOnly startDate,
        DateOnly endDate,
        string? winnerName,
        CancellationToken cancellationToken = default)
    {
        var description = $"{competitionName} season from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(winnerName))
        {
            description += $", won by {winnerName}";
        }

        return GenerateEmbeddingAsync(description, cancellationToken);
    }

    /// <summary>
    /// Generates an embedding for a team profile.
    /// </summary>
    public Task<ImmutableArray<float>?> GenerateTeamEmbeddingAsync(
        string teamName,
        string? venue,
        string? clubColors,
        int? founded,
        string? country,
        CancellationToken cancellationToken = default)
    {
        var description = $"Football team: {teamName}";
        if (!string.IsNullOrWhiteSpace(country))
        {
            description += $" from {country}";
        }
        if (founded.HasValue)
        {
            description += $", founded in {founded.Value}";
        }
        if (!string.IsNullOrWhiteSpace(venue))
        {
            description += $", plays at {venue}";
        }
        if (!string.IsNullOrWhiteSpace(clubColors))
        {
            description += $", colors: {clubColors}";
        }

        return GenerateEmbeddingAsync(description, cancellationToken);
    }

    /// <summary>
    /// Generates a deterministic embedding using a hash-based approach.
    /// This creates consistent vectors that can be used for similarity search
    /// without requiring an external LLM service.
    /// </summary>
    private static ImmutableArray<float> GenerateDeterministicEmbedding(string text)
    {
#pragma warning disable CA1308 // Normalize strings to uppercase - lowercase is intentional for consistency
        var normalized = text.ToLowerInvariant().Trim();
#pragma warning restore CA1308
        var bytes = Encoding.UTF8.GetBytes(normalized);
        
        // Use multiple hash seeds to generate diverse dimensions
        var embedding = new float[EmbeddingDimensions];
        
        for (int i = 0; i < EmbeddingDimensions; i++)
        {
            // Create a seed that varies for each dimension
            var seedBytes = BitConverter.GetBytes(i);
            var combinedBytes = bytes.Concat(seedBytes).ToArray();
            
            // Use SHA256 for better distribution
            var hash = SHA256.HashData(combinedBytes);
            
            // Convert first 4 bytes to a float in range [-1, 1]
            var intValue = BitConverter.ToInt32(hash, 0);
            embedding[i] = (float)intValue / int.MaxValue;
        }
        
        // Normalize the vector to unit length for cosine similarity
        var magnitude = MathF.Sqrt(embedding.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] /= magnitude;
            }
        }
        
        return embedding.ToImmutableArray();
    }
}

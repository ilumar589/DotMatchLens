using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json;
using DotMatchLens.Core.Services;
using DotMatchLens.Predictions.Logging;

namespace DotMatchLens.Predictions.Services;

/// <summary>
/// Configuration options for vector embeddings.
/// </summary>
public sealed class VectorEmbeddingOptions
{
    public const string SectionName = "VectorEmbeddings";

    /// <summary>
    /// The dimensions of the embedding vectors.
    /// </summary>
    public int Dimensions { get; set; } = 768;

    /// <summary>
    /// The model to use for generating embeddings.
    /// </summary>
    public string Model { get; set; } = "nomic-embed-text";
}

/// <summary>
/// Service for generating vector embeddings using Ollama.
/// For testing, inject a custom HttpMessageHandler via HttpClientFactory configuration.
/// </summary>
public sealed class VectorEmbeddingService : IEmbeddingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<VectorEmbeddingService> _logger;
    private readonly VectorEmbeddingOptions _options;

    public VectorEmbeddingService(
        HttpClient httpClient,
        ILogger<VectorEmbeddingService> logger,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        _httpClient = httpClient;
        _logger = logger;
        _options = configuration.GetSection(VectorEmbeddingOptions.SectionName).Get<VectorEmbeddingOptions>()
            ?? new VectorEmbeddingOptions();
    }

    /// <summary>
    /// Generates an embedding for the given text using Ollama's embedding model.
    /// </summary>
    public async Task<ImmutableArray<float>?> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        try
        {
            PredictionLogMessages.LogGeneratingEmbedding(_logger, text.Length);

            var request = new
            {
                model = _options.Model,
                prompt = text
            };

            var response = await _httpClient.PostAsJsonAsync(
                new Uri("/api/embeddings", UriKind.Relative),
                request,
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                PredictionLogMessages.LogEmbeddingError(_logger, $"HTTP {response.StatusCode}", null);
                return null;
            }

            var result = await response.Content
                .ReadFromJsonAsync<EmbeddingResponse>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (result?.Embedding is null)
            {
                return null;
            }

            PredictionLogMessages.LogEmbeddingGenerated(_logger, result.Embedding.Length);
            return [.. result.Embedding];
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogEmbeddingError(_logger, ex.Message, ex);
            return null;
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

    private sealed record EmbeddingResponse(float[]? Embedding);
}

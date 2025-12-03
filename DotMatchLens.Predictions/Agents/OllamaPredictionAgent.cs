using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using DotMatchLens.Predictions.Logging;
using DotMatchLens.Predictions.Services;

namespace DotMatchLens.Predictions.Agents;

/// <summary>
/// Configuration options for the Ollama agent.
/// </summary>
public sealed class OllamaAgentOptions
{
    public const string SectionName = "OllamaAgent";

    /// <summary>
    /// The Ollama endpoint URL (OpenAI-compatible endpoint).
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:11434/v1";

    /// <summary>
    /// The model to use for predictions.
    /// </summary>
    public string Model { get; set; } = "llama3.2";

    /// <summary>
    /// The embedding model to use.
    /// </summary>
    public string EmbeddingModel { get; set; } = "nomic-embed-text";

    /// <summary>
    /// The API key for authentication (can be any value for Ollama).
    /// </summary>
    public string ApiKey { get; set; } = "ollama";
}

/// <summary>
/// AI agent implementation using Ollama for football match predictions.
/// Uses Microsoft Agent Framework.
/// </summary>
public sealed class OllamaPredictionAgent : IPredictionAgent
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<OllamaPredictionAgent> _logger;
    private readonly OllamaAgentOptions _options;
    private readonly HttpClient _httpClient;

    private const string PredictionSystemPrompt = """
        You are an expert football analyst. Analyze matches and provide predictions in JSON format.
        Your response must be valid JSON with the following structure:
        {
            "homeWinProbability": 0.0-1.0,
            "drawProbability": 0.0-1.0,
            "awayWinProbability": 0.0-1.0,
            "predictedHomeScore": integer or null,
            "predictedAwayScore": integer or null,
            "reasoning": "Your analysis",
            "confidence": 0.0-1.0
        }
        The probabilities must sum to 1.0.
        """;

    public OllamaPredictionAgent(
        ILogger<OllamaPredictionAgent> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _logger = logger;
        _options = configuration.GetSection(OllamaAgentOptions.SectionName).Get<OllamaAgentOptions>() ?? new OllamaAgentOptions();
        _httpClient = httpClientFactory.CreateClient("Ollama");
        _httpClient.BaseAddress = new Uri(_options.Endpoint);
    }

    /// <inheritdoc />
    public async Task<AgentPredictionResult> GeneratePredictionAsync(
        string homeTeamName,
        string awayTeamName,
        DateTime matchDate,
        string? additionalContext = null,
        CancellationToken cancellationToken = default)
    {
        PredictionLogMessages.LogAgentInvoked(_logger, _options.Model);
        var stopwatch = Stopwatch.StartNew();

        var prompt = $"""
            Analyze this football match and provide a prediction:
            Home Team: {homeTeamName}
            Away Team: {awayTeamName}
            Match Date: {matchDate:yyyy-MM-dd}
            {(string.IsNullOrEmpty(additionalContext) ? "" : $"Additional Context: {additionalContext}")}
            
            Provide your prediction in the required JSON format.
            """;

        try
        {
            var response = await SendChatRequestAsync(prompt, PredictionSystemPrompt, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();
            PredictionLogMessages.LogAgentResponseReceived(_logger, stopwatch.ElapsedMilliseconds);

            // Parse the JSON response
            var prediction = ParsePredictionResponse(response);

            // Generate embedding for the context
            var contextForEmbedding = $"{homeTeamName} vs {awayTeamName} {matchDate:yyyy-MM-dd} {additionalContext}";
            var embedding = await GenerateEmbeddingAsync(contextForEmbedding, cancellationToken).ConfigureAwait(false);

            return new AgentPredictionResult(
                prediction.HomeWinProbability,
                prediction.DrawProbability,
                prediction.AwayWinProbability,
                prediction.PredictedHomeScore,
                prediction.PredictedAwayScore,
                prediction.Reasoning,
                prediction.Confidence,
                _options.Model,
                embedding);
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogAgentQueryError(_logger, ex.Message, ex);
            
            // Return a default prediction if the agent fails
            return new AgentPredictionResult(
                0.33f, 0.34f, 0.33f,
                null, null,
                "Prediction unavailable - agent error",
                0.1f,
                _options.Model,
                null);
        }
    }

    /// <inheritdoc />
    public async Task<AgentQueryResult> QueryAsync(
        string query,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        PredictionLogMessages.LogAgentInvoked(_logger, _options.Model);
        var stopwatch = Stopwatch.StartNew();

        var systemPrompt = """
            You are an expert football analyst. Answer questions about football matches, 
            teams, players, and provide analysis based on your knowledge.
            Be helpful, accurate, and concise in your responses.
            """;

        var fullPrompt = string.IsNullOrEmpty(context)
            ? query
            : $"Context: {context}\n\nQuestion: {query}";

        try
        {
            var response = await SendChatRequestAsync(fullPrompt, systemPrompt, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();
            PredictionLogMessages.LogAgentResponseReceived(_logger, stopwatch.ElapsedMilliseconds);

            return new AgentQueryResult(response, _options.Model, null);
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogAgentQueryError(_logger, ex.Message, ex);
            return new AgentQueryResult("Sorry, I couldn't process your query at this time.", _options.Model, null);
        }
    }

    private async Task<string> SendChatRequestAsync(
        string prompt,
        string systemPrompt,
        CancellationToken cancellationToken)
    {
        var request = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = prompt }
            },
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);
        return result?.Message?.Content ?? string.Empty;
    }

    private async Task<ImmutableArray<float>?> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        try
        {
            var request = new
            {
                model = _options.EmbeddingModel,
                prompt = text
            };

            var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request, cancellationToken).ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);
            return result?.Embedding is not null ? [.. result.Embedding] : null;
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogEmbeddingError(_logger, ex.Message, ex);
            return null;
        }
    }

    private static PredictionJsonResponse ParsePredictionResponse(string response)
    {
        try
        {
            // Try to extract JSON from the response
            var jsonStart = response.IndexOf('{', StringComparison.Ordinal);
            var jsonEnd = response.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonString = response[jsonStart..(jsonEnd + 1)];
                var prediction = JsonSerializer.Deserialize<PredictionJsonResponse>(jsonString, JsonOptions);

                return prediction ?? GetDefaultPrediction();
            }
        }
        catch
        {
            // If parsing fails, return default
        }

        return GetDefaultPrediction();
    }

    private static PredictionJsonResponse GetDefaultPrediction()
    {
        return new PredictionJsonResponse(
            0.33f, 0.34f, 0.33f,
            null, null,
            "Unable to parse prediction response",
            0.1f);
    }

    private sealed record OllamaChatResponse(OllamaChatMessage? Message);
    private sealed record OllamaChatMessage(string? Content);
    private sealed record OllamaEmbeddingResponse(float[]? Embedding);
    private sealed record PredictionJsonResponse(
        float HomeWinProbability,
        float DrawProbability,
        float AwayWinProbability,
        int? PredictedHomeScore,
        int? PredictedAwayScore,
        string? Reasoning,
        float Confidence);
}

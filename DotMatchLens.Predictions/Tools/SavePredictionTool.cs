using System.ComponentModel;
using DotMatchLens.Data.Context;
using DotMatchLens.Data.Entities;
using DotMatchLens.Predictions.Logging;
using Pgvector;

namespace DotMatchLens.Predictions.Tools;

/// <summary>
/// MCP Tool for saving prediction results to the database.
/// </summary>
public sealed class SavePredictionTool
{
    private readonly FootballDbContext _context;
    private readonly ILogger<SavePredictionTool> _logger;

    public SavePredictionTool(FootballDbContext context, ILogger<SavePredictionTool> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Saves a match prediction to the database.
    /// </summary>
    [Description("Saves a match prediction to the database")]
    public async Task<SavePredictionResult> SavePredictionAsync(
        [Description("Match ID")] Guid matchId,
        [Description("Home win probability (0-1)")] float homeWinProbability,
        [Description("Draw probability (0-1)")] float drawProbability,
        [Description("Away win probability (0-1)")] float awayWinProbability,
        [Description("Predicted home score")] int? predictedHomeScore,
        [Description("Predicted away score")] int? predictedAwayScore,
        [Description("Reasoning for the prediction")] string? reasoning,
        [Description("Confidence score (0-1)")] float confidence,
        [Description("Model version identifier")] string? modelVersion,
        [Description("Context embedding vector")] float[]? contextEmbedding,
        CancellationToken cancellationToken = default)
    {
        PredictionLogMessages.LogToolExecuting(_logger, nameof(SavePredictionTool));

        try
        {
            Vector? embedding = null;
            if (contextEmbedding != null && contextEmbedding.Length > 0)
            {
                embedding = new Vector(contextEmbedding);
            }

            var prediction = new MatchPrediction
            {
                Id = Guid.NewGuid(),
                MatchId = matchId,
                HomeWinProbability = homeWinProbability,
                DrawProbability = drawProbability,
                AwayWinProbability = awayWinProbability,
                PredictedHomeScore = predictedHomeScore,
                PredictedAwayScore = predictedAwayScore,
                Reasoning = reasoning,
                Confidence = confidence,
                ModelVersion = modelVersion,
                ContextEmbedding = embedding
            };

            _context.MatchPredictions.Add(prediction);
            await _context.SaveChangesAsync(cancellationToken);

            PredictionLogMessages.LogToolCompleted(_logger, nameof(SavePredictionTool));
            return new SavePredictionResult(true, prediction.Id, null);
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(SavePredictionTool), ex.Message, ex);
            return new SavePredictionResult(false, Guid.Empty, ex.Message);
        }
    }
}

/// <summary>
/// Result of saving a prediction.
/// </summary>
public sealed record SavePredictionResult(
    bool Success,
    Guid PredictionId,
    string? ErrorMessage);

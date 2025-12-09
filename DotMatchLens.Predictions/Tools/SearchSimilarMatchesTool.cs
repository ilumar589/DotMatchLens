using System.ComponentModel;
using DotMatchLens.Core.Services;
using DotMatchLens.Data.Context;
using DotMatchLens.Predictions.Logging;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace DotMatchLens.Predictions.Tools;

/// <summary>
/// MCP Tool for searching similar historical matches using pgvector.
/// </summary>
public sealed class SearchSimilarMatchesTool
{
    private readonly FootballDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<SearchSimilarMatchesTool> _logger;

    public SearchSimilarMatchesTool(
        FootballDbContext context,
        IEmbeddingService embeddingService,
        ILogger<SearchSimilarMatchesTool> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    /// <summary>
    /// Finds similar historical matches based on team context.
    /// </summary>
    [Description("Finds similar historical matches using vector similarity search")]
    public async Task<List<SimilarMatchInfo>> SearchSimilarMatchesAsync(
        [Description("Description of the match context (e.g., 'Barcelona vs Real Madrid')")] string matchContext,
        [Description("Maximum number of results")] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        PredictionLogMessages.LogToolExecuting(_logger, nameof(SearchSimilarMatchesTool));

        try
        {
            // Generate embedding for the match context
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(matchContext, cancellationToken);

            if (!queryEmbedding.HasValue)
            {
                PredictionLogMessages.LogToolFailed(_logger, nameof(SearchSimilarMatchesTool), "Failed to generate embedding", null);
                return [];
            }

            var queryVector = new Vector(queryEmbedding.Value.ToArray());

            // Search for similar match predictions (which have context embeddings)
            var similarPredictions = await _context.MatchPredictions
                .AsNoTracking()
                .Include(p => p.Match)
                    .ThenInclude(m => m!.HomeTeam)
                .Include(p => p.Match)
                    .ThenInclude(m => m!.AwayTeam)
                .Where(p => p.ContextEmbedding != null && p.Match != null)
                .OrderBy(p => p.ContextEmbedding!.CosineDistance(queryVector))
                .Take(limit)
                .Select(p => new
                {
                    p.Match!.Id,
                    p.Match.HomeTeamId,
                    HomeTeamName = p.Match.HomeTeam != null ? p.Match.HomeTeam.Name : "Unknown",
                    p.Match.AwayTeamId,
                    AwayTeamName = p.Match.AwayTeam != null ? p.Match.AwayTeam.Name : "Unknown",
                    p.Match.MatchDate,
                    p.Match.HomeScore,
                    p.Match.AwayScore,
                    p.HomeWinProbability,
                    p.DrawProbability,
                    p.AwayWinProbability,
                    Distance = p.ContextEmbedding!.CosineDistance(queryVector)
                })
                .ToListAsync(cancellationToken);

            var results = similarPredictions
                .Select(p => new SimilarMatchInfo(
                    p.Id,
                    p.HomeTeamId,
                    p.HomeTeamName,
                    p.AwayTeamId,
                    p.AwayTeamName,
                    p.MatchDate,
                    p.HomeScore,
                    p.AwayScore,
                    p.HomeWinProbability,
                    p.DrawProbability,
                    p.AwayWinProbability,
                    (float)(1.0 - p.Distance))) // Convert distance to similarity
                .ToList();

            PredictionLogMessages.LogToolCompleted(_logger, nameof(SearchSimilarMatchesTool));
            return results;
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(SearchSimilarMatchesTool), ex.Message, ex);
            return [];
        }
    }
}

/// <summary>
/// Similar match information record for MCP tool responses.
/// </summary>
public sealed record SimilarMatchInfo(
    Guid MatchId,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    DateTime MatchDate,
    int? ActualHomeScore,
    int? ActualAwayScore,
    float PredictedHomeWinProbability,
    float PredictedDrawProbability,
    float PredictedAwayWinProbability,
    float SimilarityScore);

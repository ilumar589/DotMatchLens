using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json;
using DotMatchLens.Core.Services;
using DotMatchLens.Data.Context;
using DotMatchLens.Predictions.Logging;
using DotMatchLens.Predictions.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace DotMatchLens.Predictions.Tools;

/// <summary>
/// Football data tools for AI agent function calling using [Description] attributes.
/// Contains 4 tools: GetCompetitionHistory, FindSimilarTeams, GetSeasonStatistics, SearchCompetitions.
/// </summary>
public sealed class FootballDataTools
{
    /// <summary>
    /// Default similarity score for text-based fallback searches when vector search is unavailable.
    /// </summary>
    private const float FallbackTextSearchSimilarityScore = 0.5f;

    /// <summary>
    /// Estimated number of days per matchday for rough matchday calculations.
    /// </summary>
    private const int EstimatedDaysPerMatchday = 7;

    private readonly FootballDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<FootballDataTools> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public FootballDataTools(
        FootballDbContext context,
        IEmbeddingService embeddingService,
        ILogger<FootballDataTools> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves historical season data for a competition including winners.
    /// </summary>
    /// <param name="competitionCode">Competition code like 'PL' for Premier League, 'BL1' for Bundesliga.</param>
    /// <returns>JSON string with competition history data.</returns>
    [Description("Retrieves historical season data for a competition including all seasons and winners")]
    public async Task<string> GetCompetitionHistory(
        [Description("Competition code like 'PL' for Premier League, 'BL1' for Bundesliga")] string competitionCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(competitionCode);

        PredictionLogMessages.LogToolExecuting(_logger, nameof(GetCompetitionHistory));

        try
        {
            var competition = await _context.Competitions
                .AsNoTracking()
                .Include(c => c.Seasons)
                .FirstOrDefaultAsync(c => c.Code == competitionCode)
                ;

            if (competition is null)
            {
                PredictionLogMessages.LogToolFailed(_logger, nameof(GetCompetitionHistory), $"Competition {competitionCode} not found", null);
                return JsonSerializer.Serialize(new { error = $"Competition {competitionCode} not found" }, JsonOptions);
            }

            var seasons = competition.Seasons
                .OrderByDescending(s => s.StartDate)
                .Select(s => new CompetitionHistoryEntry(
                    s.ExternalId,
                    s.StartDate,
                    s.EndDate,
                    s.WinnerName,
                    s.WinnerExternalId,
                    s.CurrentMatchday))
                .ToImmutableArray();

            var result = new CompetitionHistoryResult(
                competition.Code,
                competition.Name,
                competition.AreaName,
                competition.Type,
                seasons);

            PredictionLogMessages.LogCompetitionHistoryRetrieved(_logger, competitionCode, seasons.Length);
            PredictionLogMessages.LogToolCompleted(_logger, nameof(GetCompetitionHistory));

            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(GetCompetitionHistory), ex.Message, ex);
            return JsonSerializer.Serialize(new { error = ex.Message }, JsonOptions);
        }
    }

    /// <summary>
    /// Finds teams similar to the given description using vector similarity search.
    /// </summary>
    /// <param name="description">Team name or description to search for.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <returns>JSON string with similar teams and similarity scores.</returns>
    [Description("Find teams similar to the given description using vector similarity search")]
    public async Task<string> FindSimilarTeams(
        [Description("Team name or description to search for")] string description,
        [Description("Maximum number of results to return")] int limit = 5)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        PredictionLogMessages.LogToolExecuting(_logger, nameof(FindSimilarTeams));

        try
        {
            // Generate embedding for the search query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(description)
                ;

            if (!queryEmbedding.HasValue)
            {
                // Fall back to text search if embedding fails
                var textResults = await _context.Teams
                    .AsNoTracking()
                    .Where(t => t.Name.Contains(description) ||
                               (t.Country != null && t.Country.Contains(description)) ||
                               (t.Venue != null && t.Venue.Contains(description)))
                    .Take(limit)
                    .Select(t => new SimilarTeamResult(
                        t.Id,
                        t.Name,
                        t.Country,
                        t.Venue,
                        t.ClubColors,
                        t.Founded,
                        FallbackTextSearchSimilarityScore))
                    .ToListAsync()
                    ;

                PredictionLogMessages.LogSimilarTeamsFound(_logger, textResults.Count);
                PredictionLogMessages.LogToolCompleted(_logger, nameof(FindSimilarTeams));
                return JsonSerializer.Serialize(textResults, JsonOptions);
            }

            var queryVector = new Vector(queryEmbedding.Value.AsMemory());

            // Query using vector similarity (cosine distance)
            var results = await _context.Teams
                .AsNoTracking()
                .Where(t => t.Embedding != null)
                .OrderBy(t => t.Embedding!.CosineDistance(queryVector))
                .Take(limit)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Country,
                    t.Venue,
                    t.ClubColors,
                    t.Founded,
                    Distance = t.Embedding!.CosineDistance(queryVector)
                })
                .ToListAsync()
                ;

            var similarTeams = results
                .Select(r => new SimilarTeamResult(
                    r.Id,
                    r.Name,
                    r.Country,
                    r.Venue,
                    r.ClubColors,
                    r.Founded,
                    (float)(1.0 - r.Distance))) // Convert distance to similarity
                .ToImmutableArray();

            PredictionLogMessages.LogSimilarTeamsFound(_logger, similarTeams.Length);
            PredictionLogMessages.LogToolCompleted(_logger, nameof(FindSimilarTeams));

            return JsonSerializer.Serialize(similarTeams, JsonOptions);
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(FindSimilarTeams), ex.Message, ex);
            return JsonSerializer.Serialize(new { error = ex.Message }, JsonOptions);
        }
    }

    /// <summary>
    /// Gets statistics for a specific season by its external ID.
    /// </summary>
    /// <param name="seasonId">The external season ID.</param>
    /// <returns>JSON string with season statistics.</returns>
    [Description("Gets statistics for a specific season by its external ID")]
    public async Task<string> GetSeasonStatistics(
        [Description("The external season ID")] int seasonId)
    {
        PredictionLogMessages.LogToolExecuting(_logger, nameof(GetSeasonStatistics));

        try
        {
            var season = await _context.Seasons
                .AsNoTracking()
                .Include(s => s.Competition)
                .FirstOrDefaultAsync(s => s.ExternalId == seasonId)
                ;

            if (season is null)
            {
                PredictionLogMessages.LogToolFailed(_logger, nameof(GetSeasonStatistics), $"Season {seasonId} not found", null);
                return JsonSerializer.Serialize(new { error = $"Season {seasonId} not found" }, JsonOptions);
            }

            var result = CreateStatisticsResult(season);
            PredictionLogMessages.LogToolCompleted(_logger, nameof(GetSeasonStatistics));
            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(GetSeasonStatistics), ex.Message, ex);
            return JsonSerializer.Serialize(new { error = ex.Message }, JsonOptions);
        }
    }

    /// <summary>
    /// Searches competitions using natural language query with semantic search.
    /// </summary>
    /// <param name="query">Natural language search query.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <returns>JSON string with matching competitions and similarity scores.</returns>
    [Description("Searches competitions using natural language query with semantic search using embeddings")]
    public async Task<string> SearchCompetitions(
        [Description("Natural language search query")] string query,
        [Description("Maximum number of results")] int limit = 5)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        PredictionLogMessages.LogToolExecuting(_logger, nameof(SearchCompetitions));

        try
        {
            // Generate embedding for the search query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query)
                ;

            if (!queryEmbedding.HasValue)
            {
                // Fall back to text search if embedding fails
                var textResults = await _context.Competitions
                    .AsNoTracking()
                    .Where(c => c.Name.Contains(query) ||
                               c.Code.Contains(query) ||
                               (c.AreaName != null && c.AreaName.Contains(query)) ||
                               (c.Type != null && c.Type.Contains(query)))
                    .Take(limit)
                    .Select(c => new CompetitionSearchResult(
                        c.Id,
                        c.Name,
                        c.Code,
                        c.Type,
                        c.AreaName,
                        FallbackTextSearchSimilarityScore))
                    .ToListAsync()
                    ;

                PredictionLogMessages.LogToolCompleted(_logger, nameof(SearchCompetitions));
                return JsonSerializer.Serialize(textResults, JsonOptions);
            }

            var queryVector = new Vector(queryEmbedding.Value.AsMemory());

            // Query using vector similarity
            var results = await _context.Competitions
                .AsNoTracking()
                .Where(c => c.Embedding != null)
                .OrderBy(c => c.Embedding!.CosineDistance(queryVector))
                .Take(limit)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Code,
                    c.Type,
                    c.AreaName,
                    Distance = c.Embedding!.CosineDistance(queryVector)
                })
                .ToListAsync()
                ;

            var competitions = results
                .Select(r => new CompetitionSearchResult(
                    r.Id,
                    r.Name,
                    r.Code,
                    r.Type,
                    r.AreaName,
                    (float)(1.0 - r.Distance)))
                .ToImmutableArray();

            PredictionLogMessages.LogToolCompleted(_logger, nameof(SearchCompetitions));
            return JsonSerializer.Serialize(competitions, JsonOptions);
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(SearchCompetitions), ex.Message, ex);
            return JsonSerializer.Serialize(new { error = ex.Message }, JsonOptions);
        }
    }

    private static SeasonStatisticsResult CreateStatisticsResult(Data.Entities.Season season)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isCompleted = today > season.EndDate || !string.IsNullOrEmpty(season.WinnerName);
        var daysRemaining = isCompleted ? 0 : Math.Max(0, season.EndDate.DayNumber - today.DayNumber);
        var totalDays = season.EndDate.DayNumber - season.StartDate.DayNumber;
        var totalMatchdays = totalDays / EstimatedDaysPerMatchday; // Rough estimate based on typical weekly scheduling

        return new SeasonStatisticsResult(
            season.ExternalId,
            season.Competition?.Name ?? "Unknown",
            season.StartDate,
            season.EndDate,
            season.CurrentMatchday,
            season.WinnerName,
            totalMatchdays,
            daysRemaining,
            isCompleted);
    }
}

using System.Collections.Immutable;
using DotMatchLens.Core.Services;
using DotMatchLens.Data.Context;
using DotMatchLens.Predictions.Logging;
using DotMatchLens.Predictions.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace DotMatchLens.Predictions.Tools;

/// <summary>
/// Tool for retrieving competition history including season winners.
/// </summary>
public sealed class GetCompetitionHistoryTool
{
    private readonly FootballDbContext _context;
    private readonly ILogger<GetCompetitionHistoryTool> _logger;

    public GetCompetitionHistoryTool(
        FootballDbContext context,
        ILogger<GetCompetitionHistoryTool> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves historical winners and season data for a competition.
    /// </summary>
    /// <param name="competitionCode">The competition code (e.g., "PL" for Premier League).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Competition history with all seasons and winners.</returns>
    public async Task<CompetitionHistoryResult?> ExecuteAsync(
        string competitionCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(competitionCode);

        PredictionLogMessages.LogToolExecuting(_logger, nameof(GetCompetitionHistoryTool));

        try
        {
            var competition = await _context.Competitions
                .Include(c => c.Seasons)
                .FirstOrDefaultAsync(c => c.Code == competitionCode, cancellationToken)
                ;

            if (competition is null)
            {
                PredictionLogMessages.LogToolFailed(_logger, nameof(GetCompetitionHistoryTool), $"Competition {competitionCode} not found", null);
                return null;
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

            PredictionLogMessages.LogCompetitionHistoryRetrieved(_logger, competitionCode, seasons.Length);
            PredictionLogMessages.LogToolCompleted(_logger, nameof(GetCompetitionHistoryTool));

            return new CompetitionHistoryResult(
                competition.Code,
                competition.Name,
                competition.AreaName,
                competition.Type,
                seasons);
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(GetCompetitionHistoryTool), ex.Message, ex);
            return null;
        }
    }
}

/// <summary>
/// Tool for finding similar teams using vector similarity search.
/// </summary>
public sealed class FindSimilarTeamsTool
{
    private readonly FootballDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<FindSimilarTeamsTool> _logger;

    public FindSimilarTeamsTool(
        FootballDbContext context,
        IEmbeddingService embeddingService,
        ILogger<FindSimilarTeamsTool> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    /// <summary>
    /// Finds teams similar to the given description using vector similarity.
    /// </summary>
    /// <param name="description">Team name or description to search for.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of similar teams with similarity scores.</returns>
    public async Task<ImmutableArray<SimilarTeamResult>> ExecuteAsync(
        string description,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        PredictionLogMessages.LogToolExecuting(_logger, nameof(FindSimilarTeamsTool));

        try
        {
            // Generate embedding for the search query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(description, cancellationToken)
                ;

            if (!queryEmbedding.HasValue)
            {
                // Fall back to text search if embedding fails
                var textResults = await _context.Teams
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
                        0.5f))
                    .ToListAsync(cancellationToken)
                    ;

                PredictionLogMessages.LogSimilarTeamsFound(_logger, textResults.Count);
                PredictionLogMessages.LogToolCompleted(_logger, nameof(FindSimilarTeamsTool));
                return [.. textResults];
            }

            var queryVector = new Vector(queryEmbedding.Value.AsMemory());

            // Query using vector similarity (cosine distance)
            var results = await _context.Teams
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
                .ToListAsync(cancellationToken)
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
            PredictionLogMessages.LogToolCompleted(_logger, nameof(FindSimilarTeamsTool));

            return similarTeams;
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(FindSimilarTeamsTool), ex.Message, ex);
            return [];
        }
    }
}

/// <summary>
/// Tool for querying season-level statistics.
/// </summary>
public sealed class SeasonStatisticsTool
{
    private readonly FootballDbContext _context;
    private readonly ILogger<SeasonStatisticsTool> _logger;

    public SeasonStatisticsTool(
        FootballDbContext context,
        ILogger<SeasonStatisticsTool> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets statistics for a specific season.
    /// </summary>
    /// <param name="seasonId">The external season ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Season statistics or null if not found.</returns>
    public async Task<SeasonStatisticsResult?> GetByIdAsync(
        int seasonId,
        CancellationToken cancellationToken = default)
    {
        PredictionLogMessages.LogToolExecuting(_logger, nameof(SeasonStatisticsTool));

        try
        {
            var season = await _context.Seasons
                .Include(s => s.Competition)
                .FirstOrDefaultAsync(s => s.ExternalId == seasonId, cancellationToken)
                ;

            if (season is null)
            {
                PredictionLogMessages.LogToolFailed(_logger, nameof(SeasonStatisticsTool), $"Season {seasonId} not found", null);
                return null;
            }

            var result = CreateStatisticsResult(season);
            PredictionLogMessages.LogToolCompleted(_logger, nameof(SeasonStatisticsTool));
            return result;
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(SeasonStatisticsTool), ex.Message, ex);
            return null;
        }
    }

    /// <summary>
    /// Gets seasons within a date range.
    /// </summary>
    public async Task<ImmutableArray<SeasonStatisticsResult>> GetByDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        PredictionLogMessages.LogToolExecuting(_logger, nameof(SeasonStatisticsTool));

        try
        {
            var seasons = await _context.Seasons
                .Include(s => s.Competition)
                .Where(s => s.StartDate >= startDate && s.EndDate <= endDate)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync(cancellationToken)
                ;

            var results = seasons.Select(CreateStatisticsResult).ToImmutableArray();
            PredictionLogMessages.LogToolCompleted(_logger, nameof(SeasonStatisticsTool));
            return results;
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(SeasonStatisticsTool), ex.Message, ex);
            return [];
        }
    }

    private static SeasonStatisticsResult CreateStatisticsResult(Data.Entities.Season season)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isCompleted = today > season.EndDate || !string.IsNullOrEmpty(season.WinnerName);
        var daysRemaining = isCompleted ? 0 : Math.Max(0, season.EndDate.DayNumber - today.DayNumber);
        var totalDays = season.EndDate.DayNumber - season.StartDate.DayNumber;
        var totalMatchdays = totalDays / 7; // Rough estimate

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

/// <summary>
/// Tool for semantic search across competitions.
/// </summary>
public sealed class CompetitionSearchTool
{
    private readonly FootballDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<CompetitionSearchTool> _logger;

    public CompetitionSearchTool(
        FootballDbContext context,
        IEmbeddingService embeddingService,
        ILogger<CompetitionSearchTool> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    /// <summary>
    /// Searches competitions using natural language query.
    /// </summary>
    /// <param name="query">Natural language search query.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Relevant competitions with similarity scores.</returns>
    public async Task<ImmutableArray<CompetitionSearchResult>> ExecuteAsync(
        string query,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        PredictionLogMessages.LogToolExecuting(_logger, nameof(CompetitionSearchTool));

        try
        {
            // Generate embedding for the search query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken)
                ;

            if (!queryEmbedding.HasValue)
            {
                // Fall back to text search if embedding fails
                var textResults = await _context.Competitions
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
                        0.5f))
                    .ToListAsync(cancellationToken)
                    ;

                PredictionLogMessages.LogToolCompleted(_logger, nameof(CompetitionSearchTool));
                return [.. textResults];
            }

            var queryVector = new Vector(queryEmbedding.Value.AsMemory());

            // Query using vector similarity
            var results = await _context.Competitions
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
                .ToListAsync(cancellationToken)
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

            PredictionLogMessages.LogToolCompleted(_logger, nameof(CompetitionSearchTool));
            return competitions;
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(CompetitionSearchTool), ex.Message, ex);
            return [];
        }
    }
}

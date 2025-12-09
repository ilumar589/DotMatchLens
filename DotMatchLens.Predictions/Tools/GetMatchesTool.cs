using System.ComponentModel;
using DotMatchLens.Data.Context;
using DotMatchLens.Data.Entities;
using DotMatchLens.Predictions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DotMatchLens.Predictions.Tools;

/// <summary>
/// MCP Tool for retrieving matches from the database.
/// </summary>
public sealed class GetMatchesTool
{
    private readonly FootballDbContext _context;
    private readonly ILogger<GetMatchesTool> _logger;

    public GetMatchesTool(FootballDbContext context, ILogger<GetMatchesTool> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves matches from the database with optional filtering.
    /// </summary>
    [Description("Retrieves matches with optional date range and status filtering")]
    public async Task<List<MatchInfo>> GetMatchesAsync(
        [Description("Optional start date (ISO 8601)")] DateTime? startDate = null,
        [Description("Optional end date (ISO 8601)")] DateTime? endDate = null,
        [Description("Optional match status filter")] string? status = null,
        [Description("Maximum number of results")] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        PredictionLogMessages.LogToolExecuting(_logger, nameof(GetMatchesTool));

        try
        {
            IQueryable<Match> query = _context.Matches
                .AsNoTracking()
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam);

            if (startDate.HasValue)
            {
                query = query.Where(m => m.MatchDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(m => m.MatchDate <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<MatchStatus>(status, true, out var matchStatus))
            {
                query = query.Where(m => m.Status == matchStatus);
            }

            var matches = await query
                .OrderBy(m => m.MatchDate)
                .Take(limit)
                .Select(m => new MatchInfo(
                    m.Id,
                    m.HomeTeamId,
                    m.HomeTeam != null ? m.HomeTeam.Name : "Unknown",
                    m.AwayTeamId,
                    m.AwayTeam != null ? m.AwayTeam.Name : "Unknown",
                    m.MatchDate,
                    m.Status.ToString(),
                    m.HomeScore,
                    m.AwayScore,
                    m.Stadium))
                .ToListAsync(cancellationToken);

            PredictionLogMessages.LogToolCompleted(_logger, nameof(GetMatchesTool));
            return matches;
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(GetMatchesTool), ex.Message, ex);
            return [];
        }
    }

    /// <summary>
    /// Retrieves a specific match by ID.
    /// </summary>
    [Description("Retrieves a specific match by its ID")]
    public async Task<MatchInfo?> GetMatchByIdAsync(
        [Description("Match ID")] Guid matchId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var match = await _context.Matches
                .AsNoTracking()
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.Id == matchId)
                .Select(m => new MatchInfo(
                    m.Id,
                    m.HomeTeamId,
                    m.HomeTeam != null ? m.HomeTeam.Name : "Unknown",
                    m.AwayTeamId,
                    m.AwayTeam != null ? m.AwayTeam.Name : "Unknown",
                    m.MatchDate,
                    m.Status.ToString(),
                    m.HomeScore,
                    m.AwayScore,
                    m.Stadium))
                .FirstOrDefaultAsync(cancellationToken);

            return match;
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(GetMatchByIdAsync), ex.Message, ex);
            return null;
        }
    }
}

/// <summary>
/// Match information record for MCP tool responses.
/// </summary>
public sealed record MatchInfo(
    Guid Id,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    DateTime MatchDate,
    string Status,
    int? HomeScore,
    int? AwayScore,
    string? Stadium);

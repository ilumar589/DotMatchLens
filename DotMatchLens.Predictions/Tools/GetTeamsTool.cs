using System.ComponentModel;
using DotMatchLens.Data.Context;
using DotMatchLens.Predictions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DotMatchLens.Predictions.Tools;

/// <summary>
/// MCP Tool for retrieving teams from the database.
/// </summary>
public sealed class GetTeamsTool
{
    private readonly FootballDbContext _context;
    private readonly ILogger<GetTeamsTool> _logger;

    public GetTeamsTool(FootballDbContext context, ILogger<GetTeamsTool> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves teams from the database with optional filtering.
    /// </summary>
    [Description("Retrieves teams from the database with optional name or country filtering")]
    public async Task<List<TeamInfo>> GetTeamsAsync(
        [Description("Optional team name filter")] string? nameFilter = null,
        [Description("Optional country filter")] string? countryFilter = null,
        [Description("Maximum number of results")] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        PredictionLogMessages.LogToolExecuting(_logger, nameof(GetTeamsTool));

        try
        {
            var query = _context.Teams.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                query = query.Where(t => t.Name.Contains(nameFilter));
            }

            if (!string.IsNullOrWhiteSpace(countryFilter))
            {
                query = query.Where(t => t.Country == countryFilter);
            }

            var teams = await query
                .Take(limit)
                .Select(t => new TeamInfo(
                    t.Id,
                    t.Name,
                    t.Country,
                    t.Venue,
                    t.Founded,
                    t.ClubColors))
                .ToListAsync(cancellationToken);

            PredictionLogMessages.LogToolCompleted(_logger, nameof(GetTeamsTool));
            return teams;
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogToolFailed(_logger, nameof(GetTeamsTool), ex.Message, ex);
            return [];
        }
    }
}

/// <summary>
/// Team information record for MCP tool responses.
/// </summary>
public sealed record TeamInfo(
    Guid Id,
    string Name,
    string? Country,
    string? Venue,
    int? Founded,
    string? ClubColors);

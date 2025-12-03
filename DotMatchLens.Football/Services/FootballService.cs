using System.Diagnostics;
using DotMatchLens.Data.Context;
using DotMatchLens.Data.Entities;
using DotMatchLens.Football.Logging;
using DotMatchLens.Football.Models;
using Microsoft.EntityFrameworkCore;

namespace DotMatchLens.Football.Services;

/// <summary>
/// Service for managing football data - teams, players, and matches.
/// </summary>
public sealed class FootballService
{
    private readonly FootballDbContext _context;
    private readonly ILogger<FootballService> _logger;

    public FootballService(FootballDbContext context, ILogger<FootballService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all teams, optionally filtered by name or country.
    /// Uses projections to readonly record structs for performance.
    /// </summary>
    public async Task<IReadOnlyList<TeamDto>> GetTeamsAsync(string? nameFilter = null, string? countryFilter = null, CancellationToken cancellationToken = default)
    {
        FootballLogMessages.LogFetchingTeams(_logger, nameFilter ?? countryFilter);
        var stopwatch = Stopwatch.StartNew();

        var query = _context.Teams.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(t => t.Name.Contains(nameFilter));
        }

        if (!string.IsNullOrWhiteSpace(countryFilter))
        {
            query = query.Where(t => t.Country == countryFilter);
        }

        var teams = await query
            .OrderBy(t => t.Name)
            .Select(t => new TeamDto(t.Id, t.Name, t.Country, t.League))
            .ToListAsync(cancellationToken);

        stopwatch.Stop();
        FootballLogMessages.LogQueryExecuted(_logger, stopwatch.ElapsedMilliseconds);

        return teams;
    }

    /// <summary>
    /// Get a team by ID.
    /// </summary>
    public async Task<TeamDto?> GetTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams
            .Where(t => t.Id == teamId)
            .Select(t => new TeamDto(t.Id, t.Name, t.Country, t.League))
            .FirstOrDefaultAsync(cancellationToken);

        if (team == default)
        {
            FootballLogMessages.LogTeamNotFound(_logger, teamId);
        }

        return team == default ? null : team;
    }

    /// <summary>
    /// Create a new team.
    /// </summary>
    public async Task<TeamDto> CreateTeamAsync(string name, string? country = null, string? league = null, CancellationToken cancellationToken = default)
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = name,
            Country = country,
            League = league
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync(cancellationToken);

        FootballLogMessages.LogTeamCreated(_logger, team.Id, team.Name);
        return new TeamDto(team.Id, team.Name, team.Country, team.League);
    }

    /// <summary>
    /// Get matches within a date range.
    /// Uses projections to readonly record structs for performance.
    /// </summary>
    public async Task<IReadOnlyList<MatchDto>> GetMatchesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var effectiveStartDate = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var effectiveEndDate = endDate ?? DateTime.UtcNow.AddMonths(1);

        FootballLogMessages.LogFetchingMatches(_logger, effectiveStartDate, effectiveEndDate);
        var stopwatch = Stopwatch.StartNew();

        var matches = await _context.Matches
            .Where(m => m.MatchDate >= effectiveStartDate && m.MatchDate <= effectiveEndDate)
            .OrderBy(m => m.MatchDate)
            .Select(m => new MatchDto(
                m.Id,
                m.HomeTeamId,
                m.HomeTeam != null ? m.HomeTeam.Name : "Unknown",
                m.AwayTeamId,
                m.AwayTeam != null ? m.AwayTeam.Name : "Unknown",
                m.MatchDate,
                m.Stadium,
                m.HomeScore,
                m.AwayScore,
                m.Status.ToString()))
            .ToListAsync(cancellationToken);

        stopwatch.Stop();
        FootballLogMessages.LogQueryExecuted(_logger, stopwatch.ElapsedMilliseconds);

        return matches;
    }

    /// <summary>
    /// Get a match by ID with full details.
    /// </summary>
    public async Task<MatchDto?> GetMatchByIdAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var match = await _context.Matches
            .Where(m => m.Id == matchId)
            .Select(m => new MatchDto(
                m.Id,
                m.HomeTeamId,
                m.HomeTeam != null ? m.HomeTeam.Name : "Unknown",
                m.AwayTeamId,
                m.AwayTeam != null ? m.AwayTeam.Name : "Unknown",
                m.MatchDate,
                m.Stadium,
                m.HomeScore,
                m.AwayScore,
                m.Status.ToString()))
            .FirstOrDefaultAsync(cancellationToken);

        if (match == default)
        {
            FootballLogMessages.LogMatchNotFound(_logger, matchId);
        }

        return match == default ? null : match;
    }

    /// <summary>
    /// Create a new match.
    /// </summary>
    public async Task<MatchDto> CreateMatchAsync(Guid homeTeamId, Guid awayTeamId, DateTime matchDate, string? stadium = null, CancellationToken cancellationToken = default)
    {
        var homeTeam = await _context.Teams.FindAsync([homeTeamId], cancellationToken)
            ?? throw new InvalidOperationException($"Home team {homeTeamId} not found");
        var awayTeam = await _context.Teams.FindAsync([awayTeamId], cancellationToken)
            ?? throw new InvalidOperationException($"Away team {awayTeamId} not found");

        var match = new Match
        {
            Id = Guid.NewGuid(),
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            MatchDate = matchDate,
            Stadium = stadium,
            Status = MatchStatus.Scheduled
        };

        _context.Matches.Add(match);
        await _context.SaveChangesAsync(cancellationToken);

        FootballLogMessages.LogMatchCreated(_logger, match.Id, homeTeam.Name, awayTeam.Name);
        return new MatchDto(
            match.Id,
            match.HomeTeamId,
            homeTeam.Name,
            match.AwayTeamId,
            awayTeam.Name,
            match.MatchDate,
            match.Stadium,
            match.HomeScore,
            match.AwayScore,
            match.Status.ToString());
    }

    /// <summary>
    /// Get players, optionally filtered by team.
    /// </summary>
    public async Task<IReadOnlyList<PlayerDto>> GetPlayersAsync(Guid? teamId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Players.AsQueryable();

        if (teamId.HasValue)
        {
            query = query.Where(p => p.TeamId == teamId.Value);
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new PlayerDto(
                p.Id,
                p.Name,
                p.Position,
                p.JerseyNumber,
                p.TeamId,
                p.Team != null ? p.Team.Name : null))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Create a new player.
    /// </summary>
    public async Task<PlayerDto> CreatePlayerAsync(string name, string? position = null, int? jerseyNumber = null, Guid? teamId = null, CancellationToken cancellationToken = default)
    {
        string? teamName = null;
        if (teamId.HasValue)
        {
            var team = await _context.Teams.FindAsync([teamId.Value], cancellationToken);
            teamName = team?.Name;
        }

        var player = new Player
        {
            Id = Guid.NewGuid(),
            Name = name,
            Position = position,
            JerseyNumber = jerseyNumber,
            TeamId = teamId
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync(cancellationToken);

        FootballLogMessages.LogPlayerCreated(_logger, player.Id, player.Name, player.TeamId);
        return new PlayerDto(player.Id, player.Name, player.Position, player.JerseyNumber, player.TeamId, teamName);
    }

    /// <summary>
    /// Get match events.
    /// </summary>
    public async Task<IReadOnlyList<MatchEventDto>> GetMatchEventsAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        return await _context.MatchEvents
            .Where(e => e.MatchId == matchId)
            .OrderBy(e => e.Minute)
            .Select(e => new MatchEventDto(
                e.Id,
                e.MatchId,
                e.EventType,
                e.Minute,
                e.Player != null ? e.Player.Name : null,
                e.Description))
            .ToListAsync(cancellationToken);
    }
}

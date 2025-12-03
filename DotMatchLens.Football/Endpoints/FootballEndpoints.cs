using DotMatchLens.Football.Models;
using DotMatchLens.Football.Services;

namespace DotMatchLens.Football.Endpoints;

/// <summary>
/// Football API endpoints registration.
/// </summary>
public static class FootballEndpoints
{
    /// <summary>
    /// Maps all football-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapFootballEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/football")
            .WithTags("Football");

        // Team endpoints
        group.MapGet("/teams", GetTeamsAsync)
            .WithName("GetTeams")
            .WithDescription("Get all teams with optional filtering");

        group.MapGet("/teams/{id:guid}", GetTeamByIdAsync)
            .WithName("GetTeamById")
            .WithDescription("Get a team by ID");

        group.MapPost("/teams", CreateTeamAsync)
            .WithName("CreateTeam")
            .WithDescription("Create a new team");

        // Player endpoints
        group.MapGet("/players", GetPlayersAsync)
            .WithName("GetPlayers")
            .WithDescription("Get all players with optional team filtering");

        group.MapPost("/players", CreatePlayerAsync)
            .WithName("CreatePlayer")
            .WithDescription("Create a new player");

        // Match endpoints
        group.MapGet("/matches", GetMatchesAsync)
            .WithName("GetMatches")
            .WithDescription("Get matches within a date range");

        group.MapGet("/matches/{id:guid}", GetMatchByIdAsync)
            .WithName("GetMatchById")
            .WithDescription("Get a match by ID");

        group.MapPost("/matches", CreateMatchAsync)
            .WithName("CreateMatch")
            .WithDescription("Create a new match");

        group.MapGet("/matches/{id:guid}/events", GetMatchEventsAsync)
            .WithName("GetMatchEvents")
            .WithDescription("Get events for a match");

        return endpoints;
    }

    private static async Task<IResult> GetTeamsAsync(
        FootballService service,
        string? name = null,
        string? country = null,
        CancellationToken cancellationToken = default)
    {
        var teams = await service.GetTeamsAsync(name, country, cancellationToken);
        return Results.Ok(teams);
    }

    private static async Task<IResult> GetTeamByIdAsync(
        Guid id,
        FootballService service,
        CancellationToken cancellationToken = default)
    {
        var team = await service.GetTeamByIdAsync(id, cancellationToken);
        return team is null ? Results.NotFound() : Results.Ok(team);
    }

    private static async Task<IResult> CreateTeamAsync(
        CreateTeamRequest request,
        FootballService service,
        CancellationToken cancellationToken = default)
    {
        var team = await service.CreateTeamAsync(request.Name, request.Country, request.League, cancellationToken);
        return Results.Created($"/api/football/teams/{team.Id}", team);
    }

    private static async Task<IResult> GetPlayersAsync(
        FootballService service,
        Guid? teamId = null,
        CancellationToken cancellationToken = default)
    {
        var players = await service.GetPlayersAsync(teamId, cancellationToken);
        return Results.Ok(players);
    }

    private static async Task<IResult> CreatePlayerAsync(
        CreatePlayerRequest request,
        FootballService service,
        CancellationToken cancellationToken = default)
    {
        var player = await service.CreatePlayerAsync(request.Name, request.Position, request.JerseyNumber, request.TeamId, cancellationToken);
        return Results.Created($"/api/football/players/{player.Id}", player);
    }

    private static async Task<IResult> GetMatchesAsync(
        FootballService service,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var matches = await service.GetMatchesAsync(startDate, endDate, cancellationToken);
        return Results.Ok(matches);
    }

    private static async Task<IResult> GetMatchByIdAsync(
        Guid id,
        FootballService service,
        CancellationToken cancellationToken = default)
    {
        var match = await service.GetMatchByIdAsync(id, cancellationToken);
        return match is null ? Results.NotFound() : Results.Ok(match);
    }

    private static async Task<IResult> CreateMatchAsync(
        CreateMatchRequest request,
        FootballService service,
        CancellationToken cancellationToken = default)
    {
        var match = await service.CreateMatchAsync(request.HomeTeamId, request.AwayTeamId, request.MatchDate, request.Stadium, cancellationToken);
        return Results.Created($"/api/football/matches/{match.Id}", match);
    }

    private static async Task<IResult> GetMatchEventsAsync(
        Guid id,
        FootballService service,
        CancellationToken cancellationToken = default)
    {
        var events = await service.GetMatchEventsAsync(id, cancellationToken);
        return Results.Ok(events);
    }
}

/// <summary>
/// Request model for creating a team.
/// </summary>
public sealed record CreateTeamRequest(
    string Name,
    string? Country = null,
    string? League = null);

/// <summary>
/// Request model for creating a player.
/// </summary>
public sealed record CreatePlayerRequest(
    string Name,
    string? Position = null,
    int? JerseyNumber = null,
    Guid? TeamId = null);

/// <summary>
/// Request model for creating a match.
/// </summary>
public sealed record CreateMatchRequest(
    Guid HomeTeamId,
    Guid AwayTeamId,
    DateTime MatchDate,
    string? Stadium = null);

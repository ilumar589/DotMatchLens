namespace DotMatchLens.Football.Models;

/// <summary>
/// Readonly record struct for team data transfer - used for projections.
/// </summary>
public readonly record struct TeamDto(
    Guid Id,
    string Name,
    string? Country,
    string? League);

/// <summary>
/// Readonly record struct for player data transfer - used for projections.
/// </summary>
public readonly record struct PlayerDto(
    Guid Id,
    string Name,
    string? Position,
    int? JerseyNumber,
    Guid? TeamId,
    string? TeamName);

/// <summary>
/// Readonly record struct for match data transfer - used for projections.
/// </summary>
public readonly record struct MatchDto(
    Guid Id,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    DateTime MatchDate,
    string? Stadium,
    int? HomeScore,
    int? AwayScore,
    string Status);

/// <summary>
/// Readonly record struct for match event data transfer - used for projections.
/// </summary>
public readonly record struct MatchEventDto(
    Guid Id,
    Guid MatchId,
    string EventType,
    int Minute,
    string? PlayerName,
    string? Description);

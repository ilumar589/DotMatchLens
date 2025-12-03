using System.Collections.Immutable;

namespace DotMatchLens.Football.Models;

/// <summary>
/// Readonly record struct for area data from football-data.org API.
/// </summary>
public readonly record struct AreaDto(
    int Id,
    string Name,
    string? Code,
    string? Flag);

/// <summary>
/// Readonly record struct for team details from football-data.org API.
/// </summary>
public readonly record struct TeamDetailsDto(
    int Id,
    string Name,
    string? ShortName,
    string? Tla,
    string? Crest,
    string? Address,
    string? Website,
    int? Founded,
    string? ClubColors,
    string? Venue,
    AreaDto? Area);

/// <summary>
/// Readonly record struct for season data from football-data.org API.
/// </summary>
public readonly record struct SeasonDto(
    int Id,
    DateOnly StartDate,
    DateOnly EndDate,
    int? CurrentMatchday,
    TeamDetailsDto? Winner,
    ImmutableArray<string>? Stages);

/// <summary>
/// Readonly record struct for competition response from football-data.org API.
/// </summary>
public readonly record struct CompetitionResponse(
    int Id,
    string Name,
    string Code,
    string? Type,
    string? Emblem,
    AreaDto? Area,
    SeasonDto? CurrentSeason,
    ImmutableArray<SeasonDto>? Seasons);

/// <summary>
/// Readonly record struct for stored competition data.
/// </summary>
public readonly record struct CompetitionDto(
    Guid Id,
    int ExternalId,
    string Name,
    string Code,
    string? Type,
    string? Emblem,
    string? AreaName,
    string? AreaCode,
    DateTime SyncedAt);

/// <summary>
/// Readonly record struct for stored season data.
/// </summary>
public readonly record struct StoredSeasonDto(
    Guid Id,
    int ExternalId,
    Guid CompetitionId,
    string CompetitionName,
    DateOnly StartDate,
    DateOnly EndDate,
    int? CurrentMatchday,
    string? WinnerName,
    int? WinnerId);

/// <summary>
/// Readonly record struct for competition sync result.
/// </summary>
public readonly record struct CompetitionSyncResult(
    bool Success,
    string Message,
    CompetitionDto? Competition,
    int SeasonsProcessed);

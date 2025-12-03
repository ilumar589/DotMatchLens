using System.Collections.Immutable;

namespace DotMatchLens.Predictions.Models;

/// <summary>
/// Readonly record struct for competition history entry.
/// </summary>
public readonly record struct CompetitionHistoryEntry(
    int SeasonExternalId,
    DateOnly StartDate,
    DateOnly EndDate,
    string? WinnerName,
    int? WinnerExternalId,
    int? CurrentMatchday);

/// <summary>
/// Readonly record struct for competition history result.
/// </summary>
public readonly record struct CompetitionHistoryResult(
    string CompetitionCode,
    string CompetitionName,
    string? AreaName,
    string? Type,
    ImmutableArray<CompetitionHistoryEntry> Seasons);

/// <summary>
/// Readonly record struct for similar team result.
/// </summary>
public readonly record struct SimilarTeamResult(
    Guid Id,
    string Name,
    string? Country,
    string? Venue,
    string? ClubColors,
    int? Founded,
    float SimilarityScore);

/// <summary>
/// Readonly record struct for season statistics.
/// </summary>
public readonly record struct SeasonStatisticsResult(
    int SeasonExternalId,
    string CompetitionName,
    DateOnly StartDate,
    DateOnly EndDate,
    int? CurrentMatchday,
    string? WinnerName,
    int TotalMatchdays,
    int DaysRemaining,
    bool IsCompleted);

/// <summary>
/// Readonly record struct for competition search result.
/// </summary>
public readonly record struct CompetitionSearchResult(
    Guid Id,
    string Name,
    string Code,
    string? Type,
    string? AreaName,
    float SimilarityScore);

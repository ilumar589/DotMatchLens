using System.Collections.Immutable;
using DotMatchLens.Predictions.Models;
using DotMatchLens.Predictions.Tools;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DotMatchLens.Predictions.Endpoints;

/// <summary>
/// Agent tool API endpoints registration using minimal API.
/// </summary>
public static class ToolEndpoints
{
    /// <summary>
    /// Maps all agent tool API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapToolEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/predictions/tools")
            .WithTags("Agent Tools");

        group.MapGet("/competition-history/{competitionCode}", GetCompetitionHistoryAsync)
            .WithName("GetCompetitionHistory")
            .WithDescription("Retrieve historical winners and season data for a competition");

        group.MapGet("/similar-teams", FindSimilarTeamsAsync)
            .WithName("FindSimilarTeams")
            .WithDescription("Find teams similar to the given description using vector similarity");

        group.MapGet("/season-statistics/{seasonId:int}", GetSeasonStatisticsAsync)
            .WithName("GetSeasonStatistics")
            .WithDescription("Get statistics for a specific season");

        group.MapGet("/season-statistics", GetSeasonsByDateRangeAsync)
            .WithName("GetSeasonsByDateRange")
            .WithDescription("Get seasons within a date range");

        group.MapGet("/search-competitions", SearchCompetitionsAsync)
            .WithName("SearchCompetitions")
            .WithDescription("Search competitions using natural language query");

        return endpoints;
    }

    private static async Task<Results<Ok<CompetitionHistoryResult>, NotFound>> GetCompetitionHistoryAsync(
        string competitionCode,
        GetCompetitionHistoryTool tool,
        CancellationToken cancellationToken = default)
    {
        var result = await tool.ExecuteAsync(competitionCode, cancellationToken)
            .ConfigureAwait(false);

        return result.HasValue
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound();
    }

    private static async Task<Ok<ImmutableArray<SimilarTeamResult>>> FindSimilarTeamsAsync(
        string description,
        FindSimilarTeamsTool tool,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var results = await tool.ExecuteAsync(description, limit, cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(results);
    }

    private static async Task<Results<Ok<SeasonStatisticsResult>, NotFound>> GetSeasonStatisticsAsync(
        int seasonId,
        SeasonStatisticsTool tool,
        CancellationToken cancellationToken = default)
    {
        var result = await tool.GetByIdAsync(seasonId, cancellationToken)
            .ConfigureAwait(false);

        return result.HasValue
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound();
    }

    private static async Task<Ok<ImmutableArray<SeasonStatisticsResult>>> GetSeasonsByDateRangeAsync(
        SeasonStatisticsTool tool,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveStartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5));
        var effectiveEndDate = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));

        var results = await tool.GetByDateRangeAsync(effectiveStartDate, effectiveEndDate, cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(results);
    }

    private static async Task<Ok<ImmutableArray<CompetitionSearchResult>>> SearchCompetitionsAsync(
        string query,
        CompetitionSearchTool tool,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var results = await tool.ExecuteAsync(query, limit, cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(results);
    }
}

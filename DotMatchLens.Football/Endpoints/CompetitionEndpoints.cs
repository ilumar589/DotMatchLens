using System.Collections.Immutable;
using DotMatchLens.Football.Models;
using DotMatchLens.Football.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DotMatchLens.Football.Endpoints;

/// <summary>
/// Competition API endpoints registration using minimal API.
/// </summary>
public static class CompetitionEndpoints
{
    /// <summary>
    /// Maps all competition-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapCompetitionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/football/competitions")
            .WithTags("Competitions");

        group.MapPost("/sync/{competitionCode}", SyncCompetitionAsync)
            .WithName("SyncCompetition")
            .WithDescription("Trigger data ingestion for a competition from football-data.org API");

        group.MapGet("/{competitionCode}", GetCompetitionAsync)
            .WithName("GetCompetition")
            .WithDescription("Retrieve stored competition data by code");

        group.MapGet("/{competitionCode}/seasons", GetSeasonsAsync)
            .WithName("GetCompetitionSeasons")
            .WithDescription("Get all seasons for a competition");

        return endpoints;
    }

    private static async Task<Results<Ok<CompetitionSyncResult>, BadRequest<CompetitionSyncResult>>> SyncCompetitionAsync(
        string competitionCode,
        FootballDataIngestionService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.SyncCompetitionAsync(competitionCode, cancellationToken)
            .ConfigureAwait(false);

        return result.Success
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }

    private static async Task<Results<Ok<CompetitionDto>, NotFound>> GetCompetitionAsync(
        string competitionCode,
        FootballDataIngestionService service,
        CancellationToken cancellationToken = default)
    {
        var competition = await service.GetCompetitionAsync(competitionCode, cancellationToken)
            .ConfigureAwait(false);

        return competition.HasValue
            ? TypedResults.Ok(competition.Value)
            : TypedResults.NotFound();
    }

    private static async Task<Ok<ImmutableArray<StoredSeasonDto>>> GetSeasonsAsync(
        string competitionCode,
        FootballDataIngestionService service,
        CancellationToken cancellationToken = default)
    {
        var seasons = await service.GetSeasonsForCompetitionAsync(competitionCode, cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(seasons);
    }
}

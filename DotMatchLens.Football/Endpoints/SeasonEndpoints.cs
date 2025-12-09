using DotMatchLens.Football.Models;
using DotMatchLens.Football.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DotMatchLens.Football.Endpoints;

/// <summary>
/// Season API endpoints registration using minimal API.
/// </summary>
public static class SeasonEndpoints
{
    /// <summary>
    /// Maps all season-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapSeasonEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/football/seasons")
            .WithTags("Seasons");

        group.MapGet("/{seasonId:int}", GetSeasonAsync)
            .WithName("GetSeason")
            .WithDescription("Get season details by external ID");

        return endpoints;
    }

    private static async Task<Results<Ok<StoredSeasonDto>, NotFound>> GetSeasonAsync(
        int seasonId,
        FootballDataIngestionService service,
        CancellationToken cancellationToken = default)
    {
        var season = await service.GetSeasonAsync(seasonId, cancellationToken)
            ;

        return season.HasValue
            ? TypedResults.Ok(season.Value)
            : TypedResults.NotFound();
    }
}

using DotMatchLens.Football.Endpoints;
using DotMatchLens.Football.Services;

namespace DotMatchLens.Football;

/// <summary>
/// Extension methods for registering the Football module.
/// </summary>
public static class FootballModuleExtensions
{
    /// <summary>
    /// Adds Football module services to the service collection.
    /// </summary>
    public static IServiceCollection AddFootballModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<FootballService>();

        return services;
    }

    /// <summary>
    /// Maps Football module endpoints.
    /// </summary>
    public static IEndpointRouteBuilder UseFootballModule(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapFootballEndpoints();

        return endpoints;
    }
}

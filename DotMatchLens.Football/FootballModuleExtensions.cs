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
    public static IServiceCollection AddFootballModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register services
        services.AddScoped<FootballService>();
        services.AddScoped<FootballDataIngestionService>();

        // Configure and register HTTP client for football-data.org API
        var options = configuration.GetSection(FootballDataApiOptions.SectionName).Get<FootballDataApiOptions>()
            ?? new FootballDataApiOptions();

        services.AddHttpClient<FootballDataApiClient>(client =>
        {
            client.BaseAddress = options.BaseUrl;
            client.Timeout = TimeSpan.FromSeconds(options.Timeout);

            // Add API token header if configured
            if (!string.IsNullOrWhiteSpace(options.ApiToken))
            {
                client.DefaultRequestHeaders.Add("X-Auth-Token", options.ApiToken);
            }
        });

        return services;
    }

    /// <summary>
    /// Maps Football module endpoints.
    /// </summary>
    public static IEndpointRouteBuilder UseFootballModule(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapFootballEndpoints();
        endpoints.MapCompetitionEndpoints();
        endpoints.MapSeasonEndpoints();

        return endpoints;
    }
}

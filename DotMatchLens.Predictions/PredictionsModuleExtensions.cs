using DotMatchLens.Predictions.Agents;
using DotMatchLens.Predictions.Endpoints;
using DotMatchLens.Predictions.Services;
using DotMatchLens.Predictions.Tools;

namespace DotMatchLens.Predictions;

/// <summary>
/// Extension methods for registering the Predictions module.
/// </summary>
public static class PredictionsModuleExtensions
{
    /// <summary>
    /// Adds Predictions module services to the service collection.
    /// </summary>
    public static IServiceCollection AddPredictionsModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register HTTP client for Ollama
        services.AddHttpClient("Ollama");

        // Register the prediction agent
        services.AddScoped<IPredictionAgent, OllamaPredictionAgent>();
        services.AddScoped<PredictionService>();

        // Register vector embedding service with HTTP client
        services.AddHttpClient<VectorEmbeddingService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:11434");
        });

        // Register agent tools
        services.AddScoped<GetCompetitionHistoryTool>();
        services.AddScoped<FindSimilarTeamsTool>();
        services.AddScoped<SeasonStatisticsTool>();
        services.AddScoped<CompetitionSearchTool>();

        return services;
    }

    /// <summary>
    /// Maps Predictions module endpoints.
    /// </summary>
    public static IEndpointRouteBuilder UsePredictionsModule(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPredictionEndpoints();
        endpoints.MapToolEndpoints();

        return endpoints;
    }
}

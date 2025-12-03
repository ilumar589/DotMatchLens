using DotMatchLens.Predictions.Agents;
using DotMatchLens.Predictions.Endpoints;
using DotMatchLens.Predictions.Services;

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

        return services;
    }

    /// <summary>
    /// Maps Predictions module endpoints.
    /// </summary>
    public static IEndpointRouteBuilder UsePredictionsModule(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPredictionEndpoints();

        return endpoints;
    }
}

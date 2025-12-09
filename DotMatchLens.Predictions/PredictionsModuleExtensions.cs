using DotMatchLens.Core.Services;
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
    public static IServiceCollection AddPredictionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register Ollama agent options
        var ollamaOptions = configuration.GetSection(OllamaAgentOptions.SectionName).Get<OllamaAgentOptions>()
            ?? new OllamaAgentOptions();
        services.AddSingleton(ollamaOptions);

        // Register HTTP client for Ollama (for LLM predictions only, not embeddings)
        services.AddHttpClient("Ollama");

        // Register the prediction agent
        services.AddScoped<IPredictionAgent, OllamaPredictionAgent>();
        services.AddScoped<PredictionService>();

        // Note: IEmbeddingService is registered in Football module
        // and provided by PgVectorEmbeddingService (no LLM required)

        // Register agent tools (legacy individual tools)
        services.AddScoped<GetCompetitionHistoryTool>();
        services.AddScoped<FindSimilarTeamsTool>();
        services.AddScoped<SeasonStatisticsTool>();
        services.AddScoped<CompetitionSearchTool>();

        // Register FootballDataTools with AIFunction pattern
        services.AddScoped<FootballDataTools>();

        // Register FootballAgentService using Microsoft Agent Framework
        services.AddScoped<FootballAgentService>();

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

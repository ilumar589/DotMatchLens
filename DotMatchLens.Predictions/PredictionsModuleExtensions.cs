using DotMatchLens.Core.Services;
using DotMatchLens.Predictions.Agents;
using DotMatchLens.Predictions.Configuration;
using DotMatchLens.Predictions.Consumers;
using DotMatchLens.Predictions.Endpoints;
using DotMatchLens.Predictions.HealthChecks;
using DotMatchLens.Predictions.Observability;
using DotMatchLens.Predictions.Sagas;
using DotMatchLens.Predictions.Services;
using DotMatchLens.Predictions.Tools;
using DotMatchLens.Predictions.UI;
using MassTransit;

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

        // Register workflow options
        services.Configure<WorkflowOptions>(configuration.GetSection(WorkflowOptions.SectionName));

        // Register HTTP client for Ollama (for LLM predictions only, not embeddings)
        services.AddHttpClient("Ollama");

        // Register observability components
        services.AddSingleton<WorkflowMetrics>();
        services.AddSingleton<WorkflowGraphBuilder>();

        // Register health checks
        services.AddHealthChecks()
            .AddCheck<WorkflowHealthCheck>("workflow_health", tags: ["predictions", "workflow"])
            .AddCheck<OllamaHealthCheck>("ollama_health", tags: ["predictions", "ollama", "ai"]);

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

        // Register MCP Tools for workflow-based predictions
        services.AddScoped<GetTeamsTool>();
        services.AddScoped<GetMatchesTool>();
        services.AddScoped<SearchSimilarMatchesTool>();
        services.AddScoped<SavePredictionTool>();

        // Register FootballDataTools with AIFunction pattern
        services.AddScoped<FootballDataTools>();

        // Register FootballAgentService using Microsoft Agent Framework
        services.AddScoped<FootballAgentService>();

        // Register MassTransit consumers and sagas
        services.AddScoped<MatchPredictionConsumer>();

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
        endpoints.MapWorkflowVisualizationEndpoints();

        return endpoints;
    }
}

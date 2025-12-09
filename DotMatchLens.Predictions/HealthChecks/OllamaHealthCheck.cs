using DotMatchLens.Predictions.Agents;
using DotMatchLens.Predictions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DotMatchLens.Predictions.HealthChecks;

/// <summary>
/// Health check for Ollama service connectivity.
/// </summary>
public sealed class OllamaHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaAgentOptions _options;
    private readonly WorkflowOptions _workflowOptions;
    private readonly ILogger<OllamaHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaHealthCheck"/> class.
    /// </summary>
    public OllamaHealthCheck(
        IHttpClientFactory httpClientFactory,
        OllamaAgentOptions options,
        IOptions<WorkflowOptions> workflowOptions,
        ILogger<OllamaHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _workflowOptions = workflowOptions?.Value ?? throw new ArgumentNullException(nameof(workflowOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("Ollama");
            httpClient.BaseAddress = new Uri(_options.Endpoint);
            httpClient.Timeout = TimeSpan.FromSeconds(_workflowOptions.OllamaHealthCheckTimeoutSeconds);

            // Try to connect to Ollama's API endpoint
            var response = await httpClient.GetAsync(new Uri("/api/tags", UriKind.Relative), cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var data = new Dictionary<string, object>
                {
                    { "endpoint", _options.Endpoint },
                    { "model", _options.Model },
                    { "status", "connected" },
                    { "timestamp", DateTime.UtcNow }
                };

                return HealthCheckResult.Healthy("Ollama service is accessible", data);
            }
            else
            {
                var data = new Dictionary<string, object>
                {
                    { "endpoint", _options.Endpoint },
                    { "status_code", (int)response.StatusCode },
                    { "timestamp", DateTime.UtcNow }
                };

                return HealthCheckResult.Degraded(
                    $"Ollama service returned status code {response.StatusCode}",
                    data: data);
            }
        }
        catch (HttpRequestException ex)
        {
#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogWarning(ex, "Ollama health check failed - connection error");
#pragma warning restore CA1848
            
            var data = new Dictionary<string, object>
            {
                { "endpoint", _options.Endpoint },
                { "error", ex.Message },
                { "timestamp", DateTime.UtcNow }
            };

            return HealthCheckResult.Unhealthy(
                "Ollama service is not accessible",
                ex,
                data);
        }
        catch (TaskCanceledException ex)
        {
#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogWarning(ex, "Ollama health check timed out");
#pragma warning restore CA1848
            
            var data = new Dictionary<string, object>
            {
                { "endpoint", _options.Endpoint },
                { "timeout_seconds", _workflowOptions.OllamaHealthCheckTimeoutSeconds },
                { "timestamp", DateTime.UtcNow }
            };

            return HealthCheckResult.Unhealthy(
                "Ollama service health check timed out",
                ex,
                data);
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogError(ex, "Ollama health check failed unexpectedly");
#pragma warning restore CA1848
            
            return HealthCheckResult.Unhealthy(
                "Ollama service health check failed",
                ex);
        }
    }
}

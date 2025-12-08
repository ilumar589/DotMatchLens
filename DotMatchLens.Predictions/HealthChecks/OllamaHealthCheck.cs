using DotMatchLens.Predictions.Agents;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotMatchLens.Predictions.HealthChecks;

/// <summary>
/// Health check for the Ollama AI service connectivity.
/// </summary>
public sealed class OllamaHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaAgentOptions _options;

    public OllamaHealthCheck(
        IHttpClientFactory httpClientFactory,
        OllamaAgentOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Ollama");
            client.BaseAddress = new Uri(_options.Endpoint);
            client.Timeout = TimeSpan.FromSeconds(5);

            // Check Ollama API availability by listing models
            var requestUri = new Uri("/api/tags", UriKind.Relative);
            var response = await client.GetAsync(requestUri, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy(
                    "Ollama service is available.",
                    data: new Dictionary<string, object>
                    {
                        ["Endpoint"] = _options.Endpoint,
                        ["Model"] = _options.Model
                    });
            }

            return HealthCheckResult.Degraded(
                $"Ollama service returned {response.StatusCode}.",
                data: new Dictionary<string, object>
                {
                    ["StatusCode"] = (int)response.StatusCode,
                    ["Endpoint"] = _options.Endpoint
                });
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Degraded(
                "Ollama service request timed out.",
                data: new Dictionary<string, object>
                {
                    ["Endpoint"] = _options.Endpoint
                });
        }
        catch (HttpRequestException)
        {
            return HealthCheckResult.Degraded(
                "Cannot connect to Ollama service.",
                data: new Dictionary<string, object>
                {
                    ["Endpoint"] = _options.Endpoint
                });
        }
    }
}

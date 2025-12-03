using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotMatchLens.Football.Logging;
using DotMatchLens.Football.Models;

namespace DotMatchLens.Football.Services;

/// <summary>
/// HTTP client for the football-data.org API.
/// For testing, inject a custom HttpMessageHandler via HttpClientFactory configuration.
/// </summary>
public sealed class FootballDataApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<FootballDataApiClient> _logger;

    public FootballDataApiClient(
        HttpClient httpClient,
        ILogger<FootballDataApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Fetches competition data by competition code.
    /// </summary>
    public async Task<CompetitionResponse?> GetCompetitionAsync(
        string competitionCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(competitionCode);

        FootballLogMessages.LogFetchingCompetition(_logger, competitionCode);

        try
        {
            var requestUri = new Uri($"/v4/competitions/{competitionCode}", UriKind.Relative);
            var response = await _httpClient.GetAsync(
                requestUri,
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response, competitionCode, cancellationToken).ConfigureAwait(false);
                return null;
            }

            var competition = await response.Content
                .ReadFromJsonAsync<CompetitionResponse>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            FootballLogMessages.LogCompetitionFetched(_logger, competitionCode, competition.Name);
            return competition;
        }
        catch (HttpRequestException ex)
        {
            FootballLogMessages.LogApiError(_logger, competitionCode, ex.Message, ex);
            return null;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            FootballLogMessages.LogApiTimeout(_logger, competitionCode);
            return null;
        }
    }

    /// <summary>
    /// Fetches the raw JSON response for a competition.
    /// </summary>
    public async Task<string?> GetCompetitionRawJsonAsync(
        string competitionCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(competitionCode);

        try
        {
            var requestUri = new Uri($"/v4/competitions/{competitionCode}", UriKind.Relative);
            var response = await _httpClient.GetAsync(
                requestUri,
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(response, competitionCode, cancellationToken).ConfigureAwait(false);
                return null;
            }

            return await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            FootballLogMessages.LogApiError(_logger, competitionCode, ex.Message, ex);
            return null;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            FootballLogMessages.LogApiTimeout(_logger, competitionCode);
            return null;
        }
    }

    private async Task HandleErrorResponseAsync(
        HttpResponseMessage response,
        string competitionCode,
        CancellationToken cancellationToken)
    {
        var errorContent = await response.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        switch (response.StatusCode)
        {
            case HttpStatusCode.NotFound:
                FootballLogMessages.LogCompetitionNotFound(_logger, competitionCode);
                break;
            case HttpStatusCode.TooManyRequests:
                FootballLogMessages.LogRateLimitExceeded(_logger, competitionCode);
                break;
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                FootballLogMessages.LogAuthenticationError(_logger, competitionCode);
                break;
            default:
                FootballLogMessages.LogApiError(_logger, competitionCode, $"Status: {response.StatusCode}, Body: {errorContent}", null);
                break;
        }
    }
}

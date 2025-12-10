using DotMatchLens.Predictions.Models;

namespace DotMatchLens.WebUI.Services;

/// <summary>
/// HTTP client service for Predictions module API endpoints.
/// </summary>
public sealed class PredictionsApiService
{
    private readonly HttpClient _httpClient;

    public PredictionsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PredictionDto?> GeneratePredictionAsync(Guid matchId, string? additionalContext = null, CancellationToken cancellationToken = default)
    {
        var request = new GeneratePredictionRequest(matchId, additionalContext);
        var response = await _httpClient.PostAsJsonAsync("/api/predictions/generate", request, cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PredictionDto>(cancellationToken);
        }
        
        return null;
    }

    public async Task<List<PredictionDto>> GetPredictionsForMatchAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<List<PredictionDto>>($"/api/predictions/match/{matchId}", cancellationToken);
        return response ?? [];
    }

    public async Task<AgentResponse?> QueryAgentAsync(string query, Guid? matchId = null, CancellationToken cancellationToken = default)
    {
        var request = new QueryAgentRequest(query, matchId);
        var response = await _httpClient.PostAsJsonAsync("/api/predictions/query", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AgentResponse>(cancellationToken);
    }

    // Tool endpoints
    public async Task<CompetitionHistoryResult?> GetCompetitionHistoryAsync(string competitionCode, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CompetitionHistoryResult>($"/api/predictions/tools/competition-history/{competitionCode}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<SimilarTeamResult>> FindSimilarTeamsAsync(string description, int limit = 5, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<List<SimilarTeamResult>>($"/api/predictions/tools/similar-teams?description={Uri.EscapeDataString(description)}&limit={limit}", cancellationToken);
        return response ?? [];
    }

    public async Task<SeasonStatisticsResult?> GetSeasonStatisticsAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SeasonStatisticsResult>($"/api/predictions/tools/season-statistics/{seasonId}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<SeasonStatisticsResult>> GetSeasonsByDateRangeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (startDate.HasValue) query.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue) query.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
        var response = await _httpClient.GetFromJsonAsync<List<SeasonStatisticsResult>>($"/api/predictions/tools/season-statistics{queryString}", cancellationToken);
        return response ?? [];
    }

    public async Task<List<CompetitionSearchResult>> SearchCompetitionsAsync(string query, int limit = 5, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<List<CompetitionSearchResult>>($"/api/predictions/tools/search-competitions?query={Uri.EscapeDataString(query)}&limit={limit}", cancellationToken);
        return response ?? [];
    }
}

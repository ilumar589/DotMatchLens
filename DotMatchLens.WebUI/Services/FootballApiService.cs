using DotMatchLens.Football.Models;

namespace DotMatchLens.WebUI.Services;

/// <summary>
/// HTTP client service for Football module API endpoints.
/// </summary>
public sealed class FootballApiService
{
    private readonly HttpClient _httpClient;

    public FootballApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Teams
    public async Task<List<TeamDto>> GetTeamsAsync(string? name = null, string? country = null, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(name)) query.Add($"name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrWhiteSpace(country)) query.Add($"country={Uri.EscapeDataString(country)}");

        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
        var response = await _httpClient.GetFromJsonAsync<List<TeamDto>>($"/api/football/teams{queryString}", cancellationToken);
        return response ?? [];
    }

    public async Task<TeamDto?> GetTeamByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<TeamDto>($"/api/football/teams/{id}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<TeamDto?> CreateTeamAsync(string name, string? country, string? league, CancellationToken cancellationToken = default)
    {
        var request = new { Name = name, Country = country, League = league };
        var response = await _httpClient.PostAsJsonAsync("/api/football/teams", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TeamDto>(cancellationToken);
    }

    // Players
    public async Task<List<PlayerDto>> GetPlayersAsync(Guid? teamId = null, CancellationToken cancellationToken = default)
    {
        var queryString = teamId.HasValue ? $"?teamId={teamId.Value}" : "";
        var response = await _httpClient.GetFromJsonAsync<List<PlayerDto>>($"/api/football/players{queryString}", cancellationToken);
        return response ?? [];
    }

    public async Task<PlayerDto?> CreatePlayerAsync(string name, string? position, int? jerseyNumber, Guid? teamId, CancellationToken cancellationToken = default)
    {
        var request = new { Name = name, Position = position, JerseyNumber = jerseyNumber, TeamId = teamId };
        var response = await _httpClient.PostAsJsonAsync("/api/football/players", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlayerDto>(cancellationToken);
    }

    // Matches
    public async Task<List<MatchDto>> GetMatchesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (startDate.HasValue) query.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
        if (endDate.HasValue) query.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");

        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
        var response = await _httpClient.GetFromJsonAsync<List<MatchDto>>($"/api/football/matches{queryString}", cancellationToken);
        return response ?? [];
    }

    public async Task<MatchDto?> GetMatchByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MatchDto>($"/api/football/matches/{id}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<MatchDto?> CreateMatchAsync(Guid homeTeamId, Guid awayTeamId, DateTime matchDate, string? stadium, CancellationToken cancellationToken = default)
    {
        var request = new { HomeTeamId = homeTeamId, AwayTeamId = awayTeamId, MatchDate = matchDate, Stadium = stadium };
        var response = await _httpClient.PostAsJsonAsync("/api/football/matches", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MatchDto>(cancellationToken);
    }

    public async Task<List<MatchEventDto>> GetMatchEventsAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<List<MatchEventDto>>($"/api/football/matches/{matchId}/events", cancellationToken);
        return response ?? [];
    }

    // Competitions
    public async Task<CompetitionSyncResult?> SyncCompetitionAsync(string competitionCode, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(new Uri($"/api/football/competitions/sync/{competitionCode}", UriKind.Relative), null, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CompetitionSyncResult>(cancellationToken);
        }
        return null;
    }

    public async Task<CompetitionDto?> GetCompetitionAsync(string competitionCode, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CompetitionDto>($"/api/football/competitions/{competitionCode}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<StoredSeasonDto>> GetSeasonsForCompetitionAsync(string competitionCode, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<List<StoredSeasonDto>>($"/api/football/competitions/{competitionCode}/seasons", cancellationToken);
        return response ?? [];
    }
}

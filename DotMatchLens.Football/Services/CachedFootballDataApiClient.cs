using DotMatchLens.Core.Services;
using DotMatchLens.Football.Models;

namespace DotMatchLens.Football.Services;

/// <summary>
/// Caching decorator for FootballDataApiClient that implements cache-first pattern.
/// </summary>
public sealed class CachedFootballDataApiClient
{
    private readonly FootballDataApiClient _apiClient;
    private readonly ICacheService _cacheService;

    public CachedFootballDataApiClient(
        FootballDataApiClient apiClient,
        ICacheService cacheService)
    {
        _apiClient = apiClient;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Fetches competition data with cache-first pattern.
    /// </summary>
    public async Task<CompetitionResponse?> GetCompetitionAsync(
        string competitionCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(competitionCode);

        var cacheKey = GenerateCacheKey("competition", competitionCode);

        // 1. Check cache first
        var cachedData = await _cacheService.GetAsync<CachedCompetitionResponse>(cacheKey, cancellationToken)
            ;

        if (cachedData is not null)
        {
            return cachedData.Data;
        }

        // 2. Cache miss - call API
        var apiData = await _apiClient.GetCompetitionAsync(competitionCode, cancellationToken)
            ;

        // 3. Store in cache if successful
        if (apiData is not null)
        {
            await _cacheService.SetAsync(cacheKey, new CachedCompetitionResponse { Data = apiData.Value }, cancellationToken)
                ;
        }

        return apiData;
    }

    /// <summary>
    /// Fetches the raw JSON response for a competition with cache-first pattern.
    /// </summary>
    public async Task<string?> GetCompetitionRawJsonAsync(
        string competitionCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(competitionCode);

        var cacheKey = GenerateCacheKey("competition-raw", competitionCode);

        // 1. Check cache first
        var cachedData = await _cacheService.GetAsync<CachedString>(cacheKey, cancellationToken)
            ;

        if (cachedData is not null)
        {
            return cachedData.Value;
        }

        // 2. Cache miss - call API
        var apiData = await _apiClient.GetCompetitionRawJsonAsync(competitionCode, cancellationToken)
            ;

        // 3. Store in cache if successful
        if (apiData is not null)
        {
            await _cacheService.SetAsync(cacheKey, new CachedString { Value = apiData }, cancellationToken)
                ;
        }

        return apiData;
    }

    /// <summary>
    /// Invalidates cached competition data.
    /// </summary>
    public async Task InvalidateCompetitionCacheAsync(
        string competitionCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(competitionCode);

        await _cacheService.RemoveAsync(GenerateCacheKey("competition", competitionCode), cancellationToken)
            ;
        await _cacheService.RemoveAsync(GenerateCacheKey("competition-raw", competitionCode), cancellationToken)
            ;
    }

    /// <summary>
    /// Generates a cache key based on the resource type and identifier.
    /// </summary>
    private static string GenerateCacheKey(string resourceType, string identifier)
    {
        // Use ToUpperInvariant for normalization per CA1308
        return $"football:{resourceType}:{identifier.ToUpperInvariant()}";
    }

    /// <summary>
    /// Wrapper class for caching CompetitionResponse.
    /// </summary>
    private sealed class CachedCompetitionResponse
    {
        public required CompetitionResponse Data { get; set; }
    }

    /// <summary>
    /// Wrapper class for caching string values.
    /// </summary>
    private sealed class CachedString
    {
        public string Value { get; set; } = string.Empty;
    }
}

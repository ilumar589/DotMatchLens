using System.Collections.Immutable;
using System.Text.Json;
using DotMatchLens.Core.Services;
using DotMatchLens.Data.Context;
using DotMatchLens.Data.Entities;
using DotMatchLens.Football.Logging;
using DotMatchLens.Football.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace DotMatchLens.Football.Services;

/// <summary>
/// Service for orchestrating football data ingestion from the API to the database.
/// </summary>
public sealed class FootballDataIngestionService
{
    private readonly FootballDbContext _context;
    private readonly CachedFootballDataApiClient _apiClient;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<FootballDataIngestionService> _logger;

    public FootballDataIngestionService(
        FootballDbContext context,
        CachedFootballDataApiClient apiClient,
        IEmbeddingService embeddingService,
        ILogger<FootballDataIngestionService> logger)
    {
        _context = context;
        _apiClient = apiClient;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    /// <summary>
    /// Synchronizes competition data from the API to the database.
    /// </summary>
    public async Task<CompetitionSyncResult> SyncCompetitionAsync(
        string competitionCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(competitionCode);

        FootballLogMessages.LogCompetitionSyncStarted(_logger, competitionCode);

        try
        {
            // Fetch raw JSON and parsed response
            var rawJson = await _apiClient.GetCompetitionRawJsonAsync(competitionCode, cancellationToken)
                ;

            if (string.IsNullOrEmpty(rawJson))
            {
                FootballLogMessages.LogCompetitionSyncFailed(_logger, competitionCode, "Failed to fetch data from API", null);
                return new CompetitionSyncResult(false, "Failed to fetch competition data from API", null, 0);
            }

            var competitionResponse = await _apiClient.GetCompetitionAsync(competitionCode, cancellationToken)
                ;

            if (competitionResponse is null)
            {
                FootballLogMessages.LogCompetitionSyncFailed(_logger, competitionCode, "Failed to parse API response", null);
                return new CompetitionSyncResult(false, "Failed to parse competition data", null, 0);
            }

            var response = competitionResponse.Value;

            // Generate embedding for competition
            var embedding = await _embeddingService.GenerateCompetitionEmbeddingAsync(
                response.Name,
                response.Area?.Name,
                response.Type,
                cancellationToken);

            Vector? competitionEmbedding = null;
            if (embedding.HasValue)
            {
                competitionEmbedding = new Vector(embedding.Value.ToArray());
            }

            // Check if competition already exists
            var existingCompetition = await _context.Competitions
                .AsTracking()
                .FirstOrDefaultAsync(c => c.Code == competitionCode, cancellationToken)
                ;

            Competition competition;
            if (existingCompetition is not null)
            {
                // Update existing competition
                existingCompetition.RawJson = rawJson;
                existingCompetition.UpdatedAt = DateTime.UtcNow;
                existingCompetition.SyncedAt = DateTime.UtcNow;
                existingCompetition.Embedding = competitionEmbedding;
                competition = existingCompetition;
            }
            else
            {
                // Create new competition
                competition = new Competition
                {
                    Id = Guid.NewGuid(),
                    ExternalId = response.Id,
                    Name = response.Name,
                    Code = response.Code,
                    Type = response.Type,
                    Emblem = response.Emblem,
                    AreaName = response.Area?.Name,
                    AreaCode = response.Area?.Code,
                    AreaFlag = response.Area?.Flag,
                    RawJson = rawJson,
                    Embedding = competitionEmbedding,
                    SyncedAt = DateTime.UtcNow
                };
                _context.Competitions.Add(competition);
            }

            // Process seasons
            var seasonsProcessed = 0;
            if (response.Seasons.HasValue)
            {
                seasonsProcessed = await ProcessSeasonsAsync(
                    competition.Id,
                    response.Seasons.Value,
                    cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            FootballLogMessages.LogCompetitionSyncCompleted(_logger, competitionCode, seasonsProcessed);

            var dto = new CompetitionDto(
                competition.Id,
                competition.ExternalId,
                competition.Name,
                competition.Code,
                competition.Type,
                competition.Emblem,
                competition.AreaName,
                competition.AreaCode,
                competition.SyncedAt ?? DateTime.UtcNow);

            return new CompetitionSyncResult(true, "Competition synchronized successfully", dto, seasonsProcessed);
        }
        catch (Exception ex)
        {
            FootballLogMessages.LogCompetitionSyncFailed(_logger, competitionCode, ex.Message, ex);
            return new CompetitionSyncResult(false, $"Error during sync: {ex.Message}", null, 0);
        }
    }

    /// <summary>
    /// Gets a competition by its code.
    /// </summary>
    public async Task<CompetitionDto?> GetCompetitionAsync(
        string competitionCode,
        CancellationToken cancellationToken = default)
    {
        var competition = await _context.Competitions
            .Where(c => c.Code == competitionCode)
            .Select(c => new CompetitionDto(
                c.Id,
                c.ExternalId,
                c.Name,
                c.Code,
                c.Type,
                c.Emblem,
                c.AreaName,
                c.AreaCode,
                c.SyncedAt ?? c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            ;

        return competition == default ? null : competition;
    }

    /// <summary>
    /// Gets a season by its external ID.
    /// </summary>
    public async Task<StoredSeasonDto?> GetSeasonAsync(
        int seasonId,
        CancellationToken cancellationToken = default)
    {
        var season = await _context.Seasons
            .Include(s => s.Competition)
            .Where(s => s.ExternalId == seasonId)
            .Select(s => new StoredSeasonDto(
                s.Id,
                s.ExternalId,
                s.CompetitionId,
                s.Competition != null ? s.Competition.Name : "Unknown",
                s.StartDate,
                s.EndDate,
                s.CurrentMatchday,
                s.WinnerName,
                s.WinnerExternalId))
            .FirstOrDefaultAsync(cancellationToken)
            ;

        if (season == default)
        {
            FootballLogMessages.LogSeasonNotFound(_logger, seasonId);
            return null;
        }

        FootballLogMessages.LogSeasonRetrieved(_logger, seasonId);
        return season;
    }

    /// <summary>
    /// Gets all seasons for a competition.
    /// </summary>
    public async Task<ImmutableArray<StoredSeasonDto>> GetSeasonsForCompetitionAsync(
        string competitionCode,
        CancellationToken cancellationToken = default)
    {
        var seasons = await _context.Seasons
            .Include(s => s.Competition)
            .Where(s => s.Competition != null && s.Competition.Code == competitionCode)
            .OrderByDescending(s => s.StartDate)
            .Select(s => new StoredSeasonDto(
                s.Id,
                s.ExternalId,
                s.CompetitionId,
                s.Competition != null ? s.Competition.Name : "Unknown",
                s.StartDate,
                s.EndDate,
                s.CurrentMatchday,
                s.WinnerName,
                s.WinnerExternalId))
            .ToListAsync(cancellationToken)
            ;
        
        return [.. seasons];
    }

    private async Task<int> ProcessSeasonsAsync(
        Guid competitionId,
        ImmutableArray<SeasonDto> seasons,
        CancellationToken cancellationToken)
    {
        var processed = 0;

        // Get competition name for embedding generation
        var competition = await _context.Competitions
            .AsNoTracking()
            .Where(c => c.Id == competitionId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken)
            ;

        foreach (var seasonDto in seasons)
        {
            var existingSeason = await _context.Seasons
                .AsTracking()
                .FirstOrDefaultAsync(s => s.ExternalId == seasonDto.Id, cancellationToken)
                ;

            var seasonJson = JsonSerializer.Serialize(seasonDto);

            // Generate embedding for season
            var embedding = await _embeddingService.GenerateSeasonEmbeddingAsync(
                competition ?? "Unknown",
                seasonDto.StartDate,
                seasonDto.EndDate,
                seasonDto.Winner?.Name,
                cancellationToken);

            Vector? seasonEmbedding = null;
            if (embedding.HasValue)
            {
                seasonEmbedding = new Vector(embedding.Value.ToArray());
            }

            if (existingSeason is not null)
            {
                // Update existing season
                existingSeason.CurrentMatchday = seasonDto.CurrentMatchday;
                existingSeason.WinnerExternalId = seasonDto.Winner?.Id;
                existingSeason.WinnerName = seasonDto.Winner?.Name;
                existingSeason.RawJson = seasonJson;
                existingSeason.Embedding = seasonEmbedding;
                existingSeason.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new season
                var season = new Season
                {
                    Id = Guid.NewGuid(),
                    ExternalId = seasonDto.Id,
                    CompetitionId = competitionId,
                    StartDate = seasonDto.StartDate,
                    EndDate = seasonDto.EndDate,
                    CurrentMatchday = seasonDto.CurrentMatchday,
                    WinnerExternalId = seasonDto.Winner?.Id,
                    WinnerName = seasonDto.Winner?.Name,
                    Stages = seasonDto.Stages,
                    RawJson = seasonJson,
                    Embedding = seasonEmbedding
                };
                _context.Seasons.Add(season);
            }

            processed++;
        }

        return processed;
    }
}

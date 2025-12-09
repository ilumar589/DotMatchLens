using System.Diagnostics;
using DotMatchLens.Data.Context;
using DotMatchLens.Data.Entities;
using DotMatchLens.Predictions.Logging;
using DotMatchLens.Predictions.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace DotMatchLens.Predictions.Services;

/// <summary>
/// Service for managing match predictions using AI.
/// </summary>
public sealed class PredictionService
{
    private readonly FootballDbContext _context;
    private readonly ILogger<PredictionService> _logger;
    private readonly IPredictionAgent _agent;

    public PredictionService(
        FootballDbContext context,
        ILogger<PredictionService> logger,
        IPredictionAgent agent)
    {
        _context = context;
        _logger = logger;
        _agent = agent;
    }

    /// <summary>
    /// Generate a prediction for a match using the AI agent.
    /// </summary>
    public async Task<PredictionDto> GeneratePredictionAsync(
        Guid matchId,
        string? additionalContext = null,
        CancellationToken cancellationToken = default)
    {
        PredictionLogMessages.LogGeneratingPrediction(_logger, matchId);

        // Get match details
        var match = await _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken)
            ;

        if (match is null)
        {
            PredictionLogMessages.LogMatchNotFoundForPrediction(_logger, matchId);
            throw new InvalidOperationException($"Match {matchId} not found");
        }

        var homeTeamName = match.HomeTeam?.Name ?? "Unknown";
        var awayTeamName = match.AwayTeam?.Name ?? "Unknown";

        try
        {
            // Generate prediction using the agent
            var agentResponse = await _agent.GeneratePredictionAsync(
                homeTeamName,
                awayTeamName,
                match.MatchDate,
                additionalContext,
                cancellationToken)
                ;

            // Create and save the prediction
            var prediction = new MatchPrediction
            {
                Id = Guid.NewGuid(),
                MatchId = matchId,
                HomeWinProbability = agentResponse.HomeWinProbability,
                DrawProbability = agentResponse.DrawProbability,
                AwayWinProbability = agentResponse.AwayWinProbability,
                PredictedHomeScore = agentResponse.PredictedHomeScore,
                PredictedAwayScore = agentResponse.PredictedAwayScore,
                Reasoning = agentResponse.Reasoning,
                Confidence = agentResponse.Confidence,
                ModelVersion = agentResponse.ModelVersion,
                ContextEmbedding = agentResponse.Embedding.HasValue ? new Vector(agentResponse.Embedding.Value.AsMemory()) : null
            };

            _context.MatchPredictions.Add(prediction);
            await _context.SaveChangesAsync(cancellationToken);

            PredictionLogMessages.LogPredictionGenerated(_logger, prediction.Id, matchId, prediction.Confidence);

            return new PredictionDto(
                prediction.Id,
                prediction.MatchId,
                homeTeamName,
                awayTeamName,
                prediction.HomeWinProbability,
                prediction.DrawProbability,
                prediction.AwayWinProbability,
                prediction.PredictedHomeScore,
                prediction.PredictedAwayScore,
                prediction.Reasoning,
                prediction.Confidence,
                prediction.ModelVersion,
                prediction.PredictedAt);
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogPredictionError(_logger, matchId, ex.Message, ex);
            throw;
        }
    }

    /// <summary>
    /// Get predictions for a match.
    /// </summary>
    public async Task<IReadOnlyList<PredictionDto>> GetPredictionsForMatchAsync(
        Guid matchId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MatchPredictions
            .Where(p => p.MatchId == matchId)
            .OrderByDescending(p => p.PredictedAt)
            .Select(p => new PredictionDto(
                p.Id,
                p.MatchId,
                p.Match != null && p.Match.HomeTeam != null ? p.Match.HomeTeam.Name : "Unknown",
                p.Match != null && p.Match.AwayTeam != null ? p.Match.AwayTeam.Name : "Unknown",
                p.HomeWinProbability,
                p.DrawProbability,
                p.AwayWinProbability,
                p.PredictedHomeScore,
                p.PredictedAwayScore,
                p.Reasoning,
                p.Confidence,
                p.ModelVersion,
                p.PredictedAt))
            .ToListAsync(cancellationToken)
            ;
    }

    /// <summary>
    /// Query the AI agent with a custom question.
    /// </summary>
    public async Task<AgentResponse> QueryAgentAsync(
        string query,
        Guid? matchId = null,
        CancellationToken cancellationToken = default)
    {
        string? matchContext = null;

        if (matchId.HasValue)
        {
            var match = await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == matchId.Value, cancellationToken)
                ;

            if (match is not null)
            {
                matchContext = $"Match: {match.HomeTeam?.Name ?? "Unknown"} vs {match.AwayTeam?.Name ?? "Unknown"} on {match.MatchDate:yyyy-MM-dd}";
            }
        }

        var response = await _agent.QueryAsync(query, matchContext, cancellationToken);

        return new AgentResponse(
            response.Response,
            response.ModelVersion,
            response.Confidence);
    }
}

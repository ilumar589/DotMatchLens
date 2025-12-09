using DotMatchLens.Core.Contracts;
using DotMatchLens.Predictions.Services;
using MassTransit;

namespace DotMatchLens.Predictions.Consumers;

/// <summary>
/// Consumer that handles match prediction requests.
/// </summary>
public sealed class MatchPredictionConsumer : IConsumer<MatchPredictionRequested>
{
    private readonly PredictionService _predictionService;
    private readonly ILogger<MatchPredictionConsumer> _logger;

    public MatchPredictionConsumer(
        PredictionService predictionService,
        ILogger<MatchPredictionConsumer> logger)
    {
        _predictionService = predictionService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MatchPredictionRequested> context)
    {
        ArgumentNullException.ThrowIfNull(context);

#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
        _logger.LogInformation(
            "Processing prediction request for match {MatchId} (CorrelationId: {CorrelationId})",
            context.Message.MatchId,
            context.Message.CorrelationId);
#pragma warning restore CA1848

        try
        {
            var prediction = await _predictionService.GeneratePredictionAsync(
                context.Message.MatchId,
                context.Message.AdditionalContext,
                context.CancellationToken);

            await context.Publish(new MatchPredictionCompleted
            {
                MatchId = context.Message.MatchId,
                PredictionId = prediction.Id,
                CorrelationId = context.Message.CorrelationId,
                Success = true,
                Confidence = prediction.Confidence
            });

#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogInformation(
                "Completed prediction for match {MatchId}: PredictionId={PredictionId}, Confidence={Confidence}",
                context.Message.MatchId,
                prediction.Id,
                prediction.Confidence);
#pragma warning restore CA1848
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogError(ex,
                "Error generating prediction for match {MatchId}",
                context.Message.MatchId);
#pragma warning restore CA1848

            await context.Publish(new MatchPredictionCompleted
            {
                MatchId = context.Message.MatchId,
                PredictionId = Guid.Empty,
                CorrelationId = context.Message.CorrelationId,
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }
}

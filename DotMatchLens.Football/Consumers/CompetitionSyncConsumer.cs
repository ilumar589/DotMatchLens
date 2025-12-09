using DotMatchLens.Core.Contracts;
using DotMatchLens.Football.Services;
using MassTransit;

namespace DotMatchLens.Football.Consumers;

/// <summary>
/// Consumer that handles competition synchronization requests.
/// </summary>
public sealed class CompetitionSyncConsumer : IConsumer<CompetitionSyncRequested>
{
    private readonly FootballDataIngestionService _ingestionService;
    private readonly ILogger<CompetitionSyncConsumer> _logger;

    public CompetitionSyncConsumer(
        FootballDataIngestionService ingestionService,
        ILogger<CompetitionSyncConsumer> logger)
    {
        _ingestionService = ingestionService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CompetitionSyncRequested> context)
    {
        ArgumentNullException.ThrowIfNull(context);

#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
        _logger.LogInformation(
            "Processing competition sync request for {CompetitionCode} (CorrelationId: {CorrelationId})",
            context.Message.CompetitionCode,
            context.Message.CorrelationId);
#pragma warning restore CA1848

        try
        {
            var result = await _ingestionService.SyncCompetitionAsync(
                context.Message.CompetitionCode,
                context.CancellationToken);

            await context.Publish(new CompetitionSyncCompleted
            {
                CompetitionCode = context.Message.CompetitionCode,
                CorrelationId = context.Message.CorrelationId,
                Success = result.Success,
                ErrorMessage = result.Success ? null : result.Message,
                SeasonsProcessed = result.SeasonsProcessed
            });

#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogInformation(
                "Completed competition sync for {CompetitionCode}: {Success}",
                context.Message.CompetitionCode,
                result.Success);
#pragma warning restore CA1848
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogError(ex,
                "Error processing competition sync for {CompetitionCode}",
                context.Message.CompetitionCode);
#pragma warning restore CA1848

            await context.Publish(new CompetitionSyncCompleted
            {
                CompetitionCode = context.Message.CompetitionCode,
                CorrelationId = context.Message.CorrelationId,
                Success = false,
                ErrorMessage = ex.Message,
                SeasonsProcessed = 0
            });
        }
    }
}

using System.Diagnostics;
using MassTransit;

namespace DotMatchLens.Core.Filters;

/// <summary>
/// MassTransit consume filter that adds telemetry tracking for message consumption.
/// </summary>
/// <typeparam name="T">Message type.</typeparam>
public sealed class TelemetryConsumeFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private static readonly ActivitySource ActivitySource = new("DotMatchLens.MassTransit.Consumer", "1.0.0");
    private readonly ILogger<TelemetryConsumeFilter<T>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryConsumeFilter{T}"/> class.
    /// </summary>
    public TelemetryConsumeFilter(ILogger<TelemetryConsumeFilter<T>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("telemetry-consume");
    }

    /// <inheritdoc />
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var messageType = typeof(T).Name;
        var correlationId = context.CorrelationId ?? Guid.Empty;

        using var activity = ActivitySource.StartActivity(
            $"Consume {messageType}",
            ActivityKind.Consumer);

        activity?.SetTag("messaging.system", "masstransit");
        activity?.SetTag("messaging.operation", "consume");
        activity?.SetTag("messaging.message_type", messageType);
        activity?.SetTag("messaging.correlation_id", correlationId.ToString());
        activity?.SetTag("messaging.conversation_id", context.ConversationId?.ToString() ?? "");

        var stopwatch = Stopwatch.StartNew();

        try
        {
#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogDebug(
                "Consuming message {MessageType} (CorrelationId: {CorrelationId})",
                messageType,
                correlationId);
#pragma warning restore CA1848

            await next.Send(context);

            stopwatch.Stop();

            activity?.SetTag("messaging.status", "success");
            activity?.SetTag("messaging.duration_ms", stopwatch.ElapsedMilliseconds);

#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogDebug(
                "Consumed message {MessageType} (CorrelationId: {CorrelationId}) in {DurationMs}ms",
                messageType,
                correlationId,
                stopwatch.ElapsedMilliseconds);
#pragma warning restore CA1848
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            activity?.SetTag("messaging.status", "error");
            activity?.SetTag("messaging.error_type", ex.GetType().Name);
            activity?.SetTag("messaging.error_message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

#pragma warning disable CA1848 // Use LoggerMessage delegates for performance
            _logger.LogError(ex,
                "Error consuming message {MessageType} (CorrelationId: {CorrelationId}) after {DurationMs}ms",
                messageType,
                correlationId,
                stopwatch.ElapsedMilliseconds);
#pragma warning restore CA1848

            throw;
        }
    }
}

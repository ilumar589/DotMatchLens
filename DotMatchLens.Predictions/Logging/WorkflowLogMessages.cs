namespace DotMatchLens.Predictions.Logging;

/// <summary>
/// High-performance logging for workflow operations using LoggerMessage source generators.
/// </summary>
public static partial class WorkflowLogMessages
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "Workflow started: {WorkflowType} (CorrelationId: {CorrelationId})")]
    public static partial void LogWorkflowStarted(ILogger logger, string workflowType, Guid correlationId);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Information,
        Message = "Workflow completed: {WorkflowType} (CorrelationId: {CorrelationId}) in {DurationMs}ms")]
    public static partial void LogWorkflowCompleted(ILogger logger, string workflowType, Guid correlationId, long durationMs);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Error,
        Message = "Workflow failed: {WorkflowType} (CorrelationId: {CorrelationId}) - {ErrorMessage}")]
    public static partial void LogWorkflowFailed(ILogger logger, string workflowType, Guid correlationId, string errorMessage, Exception exception);

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Information,
        Message = "Workflow step started: {StepName} in workflow {WorkflowType}")]
    public static partial void LogWorkflowStepStarted(ILogger logger, string stepName, string workflowType);

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Information,
        Message = "Workflow step completed: {StepName} in workflow {WorkflowType} in {DurationMs}ms")]
    public static partial void LogWorkflowStepCompleted(ILogger logger, string stepName, string workflowType, long durationMs);

    [LoggerMessage(
        EventId = 3006,
        Level = LogLevel.Warning,
        Message = "Workflow step failed: {StepName} in workflow {WorkflowType} - {ErrorMessage}")]
    public static partial void LogWorkflowStepFailed(ILogger logger, string stepName, string workflowType, string errorMessage, Exception? exception);

    [LoggerMessage(
        EventId = 3007,
        Level = LogLevel.Information,
        Message = "Consumer processing message: {MessageType} (CorrelationId: {CorrelationId})")]
    public static partial void LogConsumerProcessing(ILogger logger, string messageType, Guid correlationId);

    [LoggerMessage(
        EventId = 3008,
        Level = LogLevel.Information,
        Message = "Consumer completed message: {MessageType} (CorrelationId: {CorrelationId}) in {DurationMs}ms")]
    public static partial void LogConsumerCompleted(ILogger logger, string messageType, Guid correlationId, long durationMs);

    [LoggerMessage(
        EventId = 3009,
        Level = LogLevel.Error,
        Message = "Consumer failed processing message: {MessageType} (CorrelationId: {CorrelationId}) - {ErrorMessage}")]
    public static partial void LogConsumerFailed(ILogger logger, string messageType, Guid correlationId, string errorMessage, Exception exception);

    [LoggerMessage(
        EventId = 3010,
        Level = LogLevel.Information,
        Message = "Publishing message: {MessageType} (CorrelationId: {CorrelationId})")]
    public static partial void LogPublishingMessage(ILogger logger, string messageType, Guid correlationId);

    [LoggerMessage(
        EventId = 3011,
        Level = LogLevel.Debug,
        Message = "Workflow state transition: {FromState} -> {ToState} in workflow {WorkflowType}")]
    public static partial void LogWorkflowStateTransition(ILogger logger, string fromState, string toState, string workflowType);

    [LoggerMessage(
        EventId = 3012,
        Level = LogLevel.Information,
        Message = "Batch workflow started: {BatchSize} items (CorrelationId: {CorrelationId})")]
    public static partial void LogBatchWorkflowStarted(ILogger logger, int batchSize, Guid correlationId);

    [LoggerMessage(
        EventId = 3013,
        Level = LogLevel.Information,
        Message = "Batch workflow progress: {CompletedCount}/{TotalCount} completed (CorrelationId: {CorrelationId})")]
    public static partial void LogBatchWorkflowProgress(ILogger logger, int completedCount, int totalCount, Guid correlationId);

    [LoggerMessage(
        EventId = 3014,
        Level = LogLevel.Information,
        Message = "Batch workflow completed: {SuccessCount}/{TotalCount} succeeded (CorrelationId: {CorrelationId}) in {DurationMs}ms")]
    public static partial void LogBatchWorkflowCompleted(ILogger logger, int successCount, int totalCount, Guid correlationId, long durationMs);
}

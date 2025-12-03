namespace DotMatchLens.Predictions.Logging;

/// <summary>
/// High-performance logging for the Predictions module using LoggerMessage source generators.
/// </summary>
public static partial class PredictionLogMessages
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Generating prediction for match: {MatchId}")]
    public static partial void LogGeneratingPrediction(ILogger logger, Guid matchId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Prediction generated: {PredictionId} for match {MatchId} with confidence {Confidence}")]
    public static partial void LogPredictionGenerated(ILogger logger, Guid predictionId, Guid matchId, float confidence);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Information,
        Message = "Querying predictions with similarity search, embedding dimension: {Dimension}")]
    public static partial void LogSimilaritySearch(ILogger logger, int dimension);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Warning,
        Message = "Match not found for prediction: {MatchId}")]
    public static partial void LogMatchNotFoundForPrediction(ILogger logger, Guid matchId);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Error,
        Message = "Error generating prediction for match {MatchId}: {ErrorMessage}")]
    public static partial void LogPredictionError(ILogger logger, Guid matchId, string errorMessage, Exception exception);

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Information,
        Message = "Agent invoked for prediction, model: {ModelVersion}")]
    public static partial void LogAgentInvoked(ILogger logger, string modelVersion);

    [LoggerMessage(
        EventId = 2007,
        Level = LogLevel.Debug,
        Message = "Ollama agent response received in {ElapsedMs}ms")]
    public static partial void LogAgentResponseReceived(ILogger logger, long elapsedMs);

    [LoggerMessage(
        EventId = 2008,
        Level = LogLevel.Error,
        Message = "Error querying agent: {ErrorMessage}")]
    public static partial void LogAgentQueryError(ILogger logger, string errorMessage, Exception exception);

    [LoggerMessage(
        EventId = 2009,
        Level = LogLevel.Warning,
        Message = "Error generating embedding: {ErrorMessage}")]
    public static partial void LogEmbeddingError(ILogger logger, string errorMessage, Exception? exception);

    [LoggerMessage(
        EventId = 2010,
        Level = LogLevel.Debug,
        Message = "Generating embedding for text of length: {TextLength}")]
    public static partial void LogGeneratingEmbedding(ILogger logger, int textLength);

    [LoggerMessage(
        EventId = 2011,
        Level = LogLevel.Debug,
        Message = "Embedding generated with {Dimensions} dimensions")]
    public static partial void LogEmbeddingGenerated(ILogger logger, int dimensions);

    [LoggerMessage(
        EventId = 2012,
        Level = LogLevel.Information,
        Message = "Executing tool: {ToolName}")]
    public static partial void LogToolExecuting(ILogger logger, string toolName);

    [LoggerMessage(
        EventId = 2013,
        Level = LogLevel.Information,
        Message = "Tool {ToolName} completed successfully")]
    public static partial void LogToolCompleted(ILogger logger, string toolName);

    [LoggerMessage(
        EventId = 2014,
        Level = LogLevel.Error,
        Message = "Tool {ToolName} failed: {ErrorMessage}")]
    public static partial void LogToolFailed(ILogger logger, string toolName, string errorMessage, Exception? exception);

    [LoggerMessage(
        EventId = 2015,
        Level = LogLevel.Information,
        Message = "Competition history retrieved: {CompetitionCode}, {SeasonCount} seasons")]
    public static partial void LogCompetitionHistoryRetrieved(ILogger logger, string competitionCode, int seasonCount);

    [LoggerMessage(
        EventId = 2016,
        Level = LogLevel.Information,
        Message = "Similar teams found: {Count} results")]
    public static partial void LogSimilarTeamsFound(ILogger logger, int count);
}

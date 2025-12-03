namespace DotMatchLens.Football.Logging;

/// <summary>
/// High-performance logging for the Football module using LoggerMessage source generators.
/// </summary>
public static partial class FootballLogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Fetching teams with filter: {Filter}")]
    public static partial void LogFetchingTeams(ILogger logger, string? filter);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Team created: {TeamId} - {TeamName}")]
    public static partial void LogTeamCreated(ILogger logger, Guid teamId, string teamName);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Fetching matches for date range: {StartDate} to {EndDate}")]
    public static partial void LogFetchingMatches(ILogger logger, DateTime startDate, DateTime endDate);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = "Match created: {MatchId} - {HomeTeam} vs {AwayTeam}")]
    public static partial void LogMatchCreated(ILogger logger, Guid matchId, string homeTeam, string awayTeam);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Warning,
        Message = "Team not found: {TeamId}")]
    public static partial void LogTeamNotFound(ILogger logger, Guid teamId);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Warning,
        Message = "Match not found: {MatchId}")]
    public static partial void LogMatchNotFound(ILogger logger, Guid matchId);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Error,
        Message = "Error ingesting football data: {ErrorMessage}")]
    public static partial void LogIngestionError(ILogger logger, string errorMessage, Exception exception);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Information,
        Message = "Player created: {PlayerId} - {PlayerName} for team {TeamId}")]
    public static partial void LogPlayerCreated(ILogger logger, Guid playerId, string playerName, Guid? teamId);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Debug,
        Message = "Database query executed in {ElapsedMs}ms")]
    public static partial void LogQueryExecuted(ILogger logger, long elapsedMs);
}

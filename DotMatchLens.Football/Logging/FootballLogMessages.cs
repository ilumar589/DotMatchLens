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

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Information,
        Message = "Fetching competition from API: {CompetitionCode}")]
    public static partial void LogFetchingCompetition(ILogger logger, string competitionCode);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Information,
        Message = "Competition fetched successfully: {CompetitionCode} - {CompetitionName}")]
    public static partial void LogCompetitionFetched(ILogger logger, string competitionCode, string competitionName);

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Warning,
        Message = "Competition not found: {CompetitionCode}")]
    public static partial void LogCompetitionNotFound(ILogger logger, string competitionCode);

    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Warning,
        Message = "Rate limit exceeded for competition: {CompetitionCode}")]
    public static partial void LogRateLimitExceeded(ILogger logger, string competitionCode);

    [LoggerMessage(
        EventId = 1014,
        Level = LogLevel.Error,
        Message = "Authentication error for competition: {CompetitionCode}")]
    public static partial void LogAuthenticationError(ILogger logger, string competitionCode);

    [LoggerMessage(
        EventId = 1015,
        Level = LogLevel.Error,
        Message = "API error for competition {CompetitionCode}: {ErrorMessage}")]
    public static partial void LogApiError(ILogger logger, string competitionCode, string errorMessage, Exception? exception);

    [LoggerMessage(
        EventId = 1016,
        Level = LogLevel.Warning,
        Message = "API request timeout for competition: {CompetitionCode}")]
    public static partial void LogApiTimeout(ILogger logger, string competitionCode);

    [LoggerMessage(
        EventId = 1017,
        Level = LogLevel.Information,
        Message = "Competition sync started: {CompetitionCode}")]
    public static partial void LogCompetitionSyncStarted(ILogger logger, string competitionCode);

    [LoggerMessage(
        EventId = 1018,
        Level = LogLevel.Information,
        Message = "Competition sync completed: {CompetitionCode}, seasons processed: {SeasonsCount}")]
    public static partial void LogCompetitionSyncCompleted(ILogger logger, string competitionCode, int seasonsCount);

    [LoggerMessage(
        EventId = 1019,
        Level = LogLevel.Error,
        Message = "Competition sync failed: {CompetitionCode}, error: {ErrorMessage}")]
    public static partial void LogCompetitionSyncFailed(ILogger logger, string competitionCode, string errorMessage, Exception? exception);

    [LoggerMessage(
        EventId = 1020,
        Level = LogLevel.Information,
        Message = "Season retrieved: {SeasonId}")]
    public static partial void LogSeasonRetrieved(ILogger logger, int seasonId);

    [LoggerMessage(
        EventId = 1021,
        Level = LogLevel.Warning,
        Message = "Season not found: {SeasonId}")]
    public static partial void LogSeasonNotFound(ILogger logger, int seasonId);
}

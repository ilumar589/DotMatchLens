using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DotMatchLens.Core.Services;

/// <summary>
/// Redis implementation of the cache service.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromDays(30); // 1 month
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        ArgumentNullException.ThrowIfNull(redis);
        
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var value = await _database.StringGetAsync(key);
            
            if (value.IsNullOrEmpty)
            {
                CacheLogMessages.LogCacheMiss(_logger, key);
                return null;
            }

            var result = JsonSerializer.Deserialize<T>((string)value!, JsonOptions);
            CacheLogMessages.LogCacheHit(_logger, key);
            return result;
        }
        catch (Exception ex)
        {
            CacheLogMessages.LogCacheError(_logger, "get", key, ex);
            return null;
        }
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        where T : class
    {
        return SetAsync(key, value, DefaultExpiration, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _database.StringSetAsync(key, json, expiration);
            CacheLogMessages.LogCacheSet(_logger, key, expiration.TotalSeconds);
        }
        catch (Exception ex)
        {
            CacheLogMessages.LogCacheError(_logger, "set", key, ex);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            await _database.KeyDeleteAsync(key);
            CacheLogMessages.LogCacheRemoved(_logger, key);
        }
        catch (Exception ex)
        {
            CacheLogMessages.LogCacheError(_logger, "remove", key, ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            CacheLogMessages.LogCacheError(_logger, "exists", key, ex);
            return false;
        }
    }
}

/// <summary>
/// Log messages for cache operations using source generator pattern.
/// </summary>
internal static partial class CacheLogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Cache hit for key: {Key}")]
    public static partial void LogCacheHit(ILogger logger, string key);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Cache miss for key: {Key}")]
    public static partial void LogCacheMiss(ILogger logger, string key);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Cache set for key: {Key} with expiration: {ExpirationSeconds}s")]
    public static partial void LogCacheSet(ILogger logger, string key, double expirationSeconds);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Cache removed for key: {Key}")]
    public static partial void LogCacheRemoved(ILogger logger, string key);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Cache error during {Operation} for key: {Key}")]
    public static partial void LogCacheError(ILogger logger, string operation, string key, Exception exception);
}

using DotMatchLens.Core.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DotMatchLens.Tests;

/// <summary>
/// Tests for cache service operations.
/// </summary>
public sealed class CacheServiceTests
{
    [Fact]
    public async Task GetAsync_WhenKeyNotFound_ShouldReturnNull()
    {
        // Arrange
        var redis = Substitute.For<IConnectionMultiplexer>();
        var database = Substitute.For<IDatabase>();
        redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);
        database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(RedisValue.Null);

        var logger = Substitute.For<ILogger<RedisCacheService>>();
        var service = new RedisCacheService(redis, logger);

        // Act
        var result = await service.GetAsync<TestData>("test-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ShouldNotThrow()
    {
        // Arrange
        var redis = Substitute.For<IConnectionMultiplexer>();
        var database = Substitute.For<IDatabase>();
        redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);

        var logger = Substitute.For<ILogger<RedisCacheService>>();
        var service = new RedisCacheService(redis, logger);

        var testData = new TestData { Value = "test" };

        // Act & Assert - Just verify it doesn't throw
        await service.SetAsync("test-key", testData);
    }

    [Fact]
    public async Task SetAsync_WithCustomExpiration_ShouldNotThrow()
    {
        // Arrange
        var redis = Substitute.For<IConnectionMultiplexer>();
        var database = Substitute.For<IDatabase>();
        redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);

        var logger = Substitute.For<ILogger<RedisCacheService>>();
        var service = new RedisCacheService(redis, logger);

        var testData = new TestData { Value = "test" };
        var customExpiration = TimeSpan.FromHours(1);

        // Act & Assert - Just verify it doesn't throw
        await service.SetAsync("test-key", testData, customExpiration);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallRedisKeyDelete()
    {
        // Arrange
        var redis = Substitute.For<IConnectionMultiplexer>();
        var database = Substitute.For<IDatabase>();
        redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);

        var logger = Substitute.For<ILogger<RedisCacheService>>();
        var service = new RedisCacheService(redis, logger);

        // Act
        await service.RemoveAsync("test-key");

        // Assert
        await database.Received(1).KeyDeleteAsync(
            Arg.Any<RedisKey>(),
            Arg.Any<CommandFlags>());
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        // Arrange
        var redis = Substitute.For<IConnectionMultiplexer>();
        var database = Substitute.For<IDatabase>();
        redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);
        database.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(true);

        var logger = Substitute.For<ILogger<RedisCacheService>>();
        var service = new RedisCacheService(redis, logger);

        // Act
        var result = await service.ExistsAsync("test-key");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyNotExists_ShouldReturnFalse()
    {
        // Arrange
        var redis = Substitute.For<IConnectionMultiplexer>();
        var database = Substitute.For<IDatabase>();
        redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);
        database.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(false);

        var logger = Substitute.For<ILogger<RedisCacheService>>();
        var service = new RedisCacheService(redis, logger);

        // Act
        var result = await service.ExistsAsync("test-key");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Constructor_WithNullRedis_ShouldThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger<RedisCacheService>>();

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type
        Assert.Throws<ArgumentNullException>(() => new RedisCacheService(null, logger));
#pragma warning restore CS8625
    }

    private sealed class TestData
    {
        public string Value { get; set; } = string.Empty;
    }
}

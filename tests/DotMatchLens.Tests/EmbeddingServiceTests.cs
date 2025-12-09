using System.Collections.Immutable;
using DotMatchLens.Core.Services;
using DotMatchLens.Predictions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotMatchLens.Tests;

/// <summary>
/// Tests for embedding service text generation.
/// </summary>
public sealed class EmbeddingServiceTests
{
    [Fact]
    public async Task GenerateCompetitionEmbeddingAsync_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var logger = Substitute.For<ILogger<VectorEmbeddingService>>();
        var configuration = CreateConfiguration();
        var service = new VectorEmbeddingService(httpClient, logger, configuration);

        // Act - This will fail to call actual API since we don't have Ollama running, but validates the method signature
        var result = await service.GenerateCompetitionEmbeddingAsync(
            "Premier League",
            "England",
            "LEAGUE",
            CancellationToken.None);

        // Assert - We expect null because there's no actual Ollama endpoint
        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateSeasonEmbeddingAsync_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var logger = Substitute.For<ILogger<VectorEmbeddingService>>();
        var configuration = CreateConfiguration();
        var service = new VectorEmbeddingService(httpClient, logger, configuration);

        // Act
        var result = await service.GenerateSeasonEmbeddingAsync(
            "Premier League",
            new DateOnly(2023, 8, 1),
            new DateOnly(2024, 5, 31),
            "Manchester City",
            CancellationToken.None);

        // Assert - We expect null because there's no actual Ollama endpoint
        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateTeamEmbeddingAsync_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var logger = Substitute.For<ILogger<VectorEmbeddingService>>();
        var configuration = CreateConfiguration();
        var service = new VectorEmbeddingService(httpClient, logger, configuration);

        // Act
        var result = await service.GenerateTeamEmbeddingAsync(
            "Manchester United",
            "Old Trafford",
            "Red",
            1878,
            "England",
            CancellationToken.None);

        // Assert - We expect null because there's no actual Ollama endpoint
        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithEmptyText_ShouldThrow()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var logger = Substitute.For<ILogger<VectorEmbeddingService>>();
        var configuration = CreateConfiguration();
        var service = new VectorEmbeddingService(httpClient, logger, configuration);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await service.GenerateEmbeddingAsync("", CancellationToken.None);
        });
    }

    private static IConfiguration CreateConfiguration()
    {
        var configData = new Dictionary<string, string?>
        {
            ["VectorEmbeddings:Dimensions"] = "768",
            ["VectorEmbeddings:Model"] = "nomic-embed-text"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }
}

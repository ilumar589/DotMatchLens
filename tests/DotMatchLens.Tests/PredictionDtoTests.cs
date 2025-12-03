using DotMatchLens.Predictions.Models;

namespace DotMatchLens.Tests;

/// <summary>
/// Tests for Prediction DTOs.
/// </summary>
public sealed class PredictionDtoTests
{
    [Fact]
    public void PredictionDto_ShouldBeReadonlyRecordStruct()
    {
        // Arrange & Act
        var prediction = new PredictionDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Home Team",
            "Away Team",
            0.45f,
            0.30f,
            0.25f,
            2,
            1,
            "Home team has better form",
            0.85f,
            "llama3.2",
            DateTime.UtcNow);

        // Assert
        Assert.Equal("Home Team", prediction.HomeTeamName);
        Assert.Equal("Away Team", prediction.AwayTeamName);
        Assert.Equal(0.45f, prediction.HomeWinProbability);
        Assert.Equal(0.30f, prediction.DrawProbability);
        Assert.Equal(0.25f, prediction.AwayWinProbability);
        Assert.Equal(2, prediction.PredictedHomeScore);
        Assert.Equal(1, prediction.PredictedAwayScore);
        Assert.Equal("Home team has better form", prediction.Reasoning);
        Assert.Equal(0.85f, prediction.Confidence);
        Assert.Equal("llama3.2", prediction.ModelVersion);
    }

    [Fact]
    public void PredictionDto_ProbabilitiesShouldSumToOne()
    {
        // Arrange
        var prediction = new PredictionDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Home",
            "Away",
            0.4f,
            0.35f,
            0.25f,
            null,
            null,
            null,
            0.8f,
            null,
            DateTime.UtcNow);

        // Act
        var sum = prediction.HomeWinProbability + prediction.DrawProbability + prediction.AwayWinProbability;

        // Assert
        Assert.Equal(1.0f, sum, 2);
    }

    [Fact]
    public void GeneratePredictionRequest_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var matchId = Guid.NewGuid();
        var request = new GeneratePredictionRequest(matchId, "Additional context");

        // Assert
        Assert.Equal(matchId, request.MatchId);
        Assert.Equal("Additional context", request.AdditionalContext);
    }

    [Fact]
    public void QueryAgentRequest_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var matchId = Guid.NewGuid();
        var request = new QueryAgentRequest("What is the best formation?", matchId);

        // Assert
        Assert.Equal("What is the best formation?", request.Query);
        Assert.Equal(matchId, request.MatchId);
    }

    [Fact]
    public void AgentResponse_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var response = new AgentResponse(
            "The 4-3-3 formation works well for possession-based play.",
            "llama3.2",
            0.9f);

        // Assert
        Assert.Equal("The 4-3-3 formation works well for possession-based play.", response.Response);
        Assert.Equal("llama3.2", response.ModelVersion);
        Assert.Equal(0.9f, response.Confidence);
    }
}

using System.Collections.Immutable;
using DotMatchLens.Predictions.Models;

namespace DotMatchLens.Tests;

/// <summary>
/// Tests for Agent Tool DTOs.
/// </summary>
public sealed class ToolDtoTests
{
    [Fact]
    public void CompetitionHistoryEntry_ShouldBeReadonlyRecordStruct()
    {
        // Arrange & Act
        var entry = new CompetitionHistoryEntry(
            733,
            new DateOnly(2021, 8, 13),
            new DateOnly(2022, 5, 22),
            "Manchester City FC",
            65,
            38);

        // Assert
        Assert.Equal(733, entry.SeasonExternalId);
        Assert.Equal(new DateOnly(2021, 8, 13), entry.StartDate);
        Assert.Equal(new DateOnly(2022, 5, 22), entry.EndDate);
        Assert.Equal("Manchester City FC", entry.WinnerName);
        Assert.Equal(65, entry.WinnerExternalId);
        Assert.Equal(38, entry.CurrentMatchday);
    }

    [Fact]
    public void CompetitionHistoryResult_ShouldContainSeasons()
    {
        // Arrange
        var seasons = ImmutableArray.Create(
            new CompetitionHistoryEntry(733, new DateOnly(2021, 8, 13), new DateOnly(2022, 5, 22), "Man City", 65, 38),
            new CompetitionHistoryEntry(619, new DateOnly(2020, 9, 12), new DateOnly(2021, 5, 23), "Man City", 65, 38));

        // Act
        var result = new CompetitionHistoryResult(
            "PL",
            "Premier League",
            "England",
            "LEAGUE",
            seasons);

        // Assert
        Assert.Equal("PL", result.CompetitionCode);
        Assert.Equal("Premier League", result.CompetitionName);
        Assert.Equal("England", result.AreaName);
        Assert.Equal("LEAGUE", result.Type);
        Assert.Equal(2, result.Seasons.Length);
    }

    [Fact]
    public void SimilarTeamResult_ShouldIncludeSimilarityScore()
    {
        // Arrange & Act
        var result = new SimilarTeamResult(
            Guid.NewGuid(),
            "Arsenal FC",
            "England",
            "Emirates Stadium",
            "Red / White",
            1886,
            0.95f);

        // Assert
        Assert.Equal("Arsenal FC", result.Name);
        Assert.Equal("England", result.Country);
        Assert.Equal("Emirates Stadium", result.Venue);
        Assert.Equal("Red / White", result.ClubColors);
        Assert.Equal(1886, result.Founded);
        Assert.Equal(0.95f, result.SimilarityScore);
    }

    [Fact]
    public void SeasonStatisticsResult_ShouldCalculateCompletion()
    {
        // Arrange & Act
        var result = new SeasonStatisticsResult(
            733,
            "Premier League",
            new DateOnly(2021, 8, 13),
            new DateOnly(2022, 5, 22),
            38,
            "Manchester City FC",
            38,
            0,
            true);

        // Assert
        Assert.Equal(733, result.SeasonExternalId);
        Assert.Equal("Premier League", result.CompetitionName);
        Assert.Equal(38, result.CurrentMatchday);
        Assert.Equal("Manchester City FC", result.WinnerName);
        Assert.True(result.IsCompleted);
        Assert.Equal(0, result.DaysRemaining);
    }

    [Fact]
    public void SeasonStatisticsResult_InProgress_ShouldHaveDaysRemaining()
    {
        // Arrange & Act
        var result = new SeasonStatisticsResult(
            1000,
            "Premier League",
            new DateOnly(2024, 8, 16),
            new DateOnly(2025, 5, 25),
            20,
            null,
            38,
            100,
            false);

        // Assert
        Assert.Null(result.WinnerName);
        Assert.False(result.IsCompleted);
        Assert.Equal(100, result.DaysRemaining);
        Assert.Equal(20, result.CurrentMatchday);
    }

    [Fact]
    public void CompetitionSearchResult_ShouldIncludeSimilarityScore()
    {
        // Arrange & Act
        var result = new CompetitionSearchResult(
            Guid.NewGuid(),
            "Premier League",
            "PL",
            "LEAGUE",
            "England",
            0.92f);

        // Assert
        Assert.Equal("Premier League", result.Name);
        Assert.Equal("PL", result.Code);
        Assert.Equal("LEAGUE", result.Type);
        Assert.Equal("England", result.AreaName);
        Assert.Equal(0.92f, result.SimilarityScore);
    }

    [Fact]
    public void CompetitionHistoryResult_EmptySeasons_ShouldBeValid()
    {
        // Arrange & Act
        var result = new CompetitionHistoryResult(
            "NEW",
            "New Competition",
            "Unknown",
            "LEAGUE",
            ImmutableArray<CompetitionHistoryEntry>.Empty);

        // Assert
        Assert.Equal("NEW", result.CompetitionCode);
        Assert.Empty(result.Seasons);
    }
}

using System.Collections.Immutable;
using DotMatchLens.Football.Models;

namespace DotMatchLens.Tests;

/// <summary>
/// Tests for Competition DTOs.
/// </summary>
public sealed class CompetitionDtoTests
{
    [Fact]
    public void AreaDto_ShouldBeReadonlyRecordStruct()
    {
        // Arrange & Act
        var area = new AreaDto(
            2072,
            "England",
            "ENG",
            "https://crests.football-data.org/770.svg");

        // Assert
        Assert.Equal(2072, area.Id);
        Assert.Equal("England", area.Name);
        Assert.Equal("ENG", area.Code);
        Assert.Equal("https://crests.football-data.org/770.svg", area.Flag);
    }

    [Fact]
    public void AreaDto_ShouldSupportEquality()
    {
        // Arrange
        var area1 = new AreaDto(2072, "England", "ENG", "flag.svg");
        var area2 = new AreaDto(2072, "England", "ENG", "flag.svg");

        // Assert
        Assert.Equal(area1, area2);
    }

    [Fact]
    public void SeasonDto_ShouldBeReadonlyRecordStruct()
    {
        // Arrange & Act
        var season = new SeasonDto(
            733,
            new DateOnly(2021, 8, 13),
            new DateOnly(2022, 5, 22),
            37,
            null,
            ImmutableArray.Create("REGULAR_SEASON"));

        // Assert
        Assert.Equal(733, season.Id);
        Assert.Equal(new DateOnly(2021, 8, 13), season.StartDate);
        Assert.Equal(new DateOnly(2022, 5, 22), season.EndDate);
        Assert.Equal(37, season.CurrentMatchday);
        Assert.Null(season.Winner);
        Assert.Single(season.Stages!.Value);
        Assert.Equal("REGULAR_SEASON", season.Stages!.Value[0]);
    }

    [Fact]
    public void SeasonDto_WithWinner_ShouldContainTeamDetails()
    {
        // Arrange
        var winner = new TeamDetailsDto(
            65,
            "Manchester City FC",
            "Man City",
            "MCI",
            "https://crests.football-data.org/65.png",
            null, null, 1880, "Sky Blue / White", "Etihad Stadium", null);

        // Act
        var season = new SeasonDto(
            733,
            new DateOnly(2021, 8, 13),
            new DateOnly(2022, 5, 22),
            38,
            winner,
            null);

        // Assert
        Assert.NotNull(season.Winner);
        Assert.Equal("Manchester City FC", season.Winner!.Value.Name);
        Assert.Equal(65, season.Winner.Value.Id);
    }

    [Fact]
    public void CompetitionResponse_ShouldBeReadonlyRecordStruct()
    {
        // Arrange
        var area = new AreaDto(2072, "England", "ENG", null);
        var currentSeason = new SeasonDto(
            733,
            new DateOnly(2021, 8, 13),
            new DateOnly(2022, 5, 22),
            37,
            null,
            null);

        // Act
        var competition = new CompetitionResponse(
            2021,
            "Premier League",
            "PL",
            "LEAGUE",
            "https://crests.football-data.org/PL.png",
            area,
            currentSeason,
            null);

        // Assert
        Assert.Equal(2021, competition.Id);
        Assert.Equal("Premier League", competition.Name);
        Assert.Equal("PL", competition.Code);
        Assert.Equal("LEAGUE", competition.Type);
        Assert.NotNull(competition.Area);
        Assert.Equal("England", competition.Area!.Value.Name);
        Assert.NotNull(competition.CurrentSeason);
        Assert.Equal(37, competition.CurrentSeason!.Value.CurrentMatchday);
    }

    [Fact]
    public void CompetitionDto_ShouldBeReadonlyRecordStruct()
    {
        // Arrange & Act
        var dto = new CompetitionDto(
            Guid.NewGuid(),
            2021,
            "Premier League",
            "PL",
            "LEAGUE",
            "emblem.png",
            "England",
            "ENG",
            DateTime.UtcNow);

        // Assert
        Assert.Equal("Premier League", dto.Name);
        Assert.Equal("PL", dto.Code);
        Assert.Equal(2021, dto.ExternalId);
    }

    [Fact]
    public void StoredSeasonDto_ShouldBeReadonlyRecordStruct()
    {
        // Arrange & Act
        var dto = new StoredSeasonDto(
            Guid.NewGuid(),
            733,
            Guid.NewGuid(),
            "Premier League",
            new DateOnly(2021, 8, 13),
            new DateOnly(2022, 5, 22),
            37,
            "Manchester City FC",
            65);

        // Assert
        Assert.Equal(733, dto.ExternalId);
        Assert.Equal("Premier League", dto.CompetitionName);
        Assert.Equal("Manchester City FC", dto.WinnerName);
        Assert.Equal(65, dto.WinnerId);
    }

    [Fact]
    public void CompetitionSyncResult_Success_ShouldHaveCompetition()
    {
        // Arrange
        var competition = new CompetitionDto(
            Guid.NewGuid(),
            2021,
            "Premier League",
            "PL",
            "LEAGUE",
            null, null, null,
            DateTime.UtcNow);

        // Act
        var result = new CompetitionSyncResult(true, "Success", competition, 5);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Success", result.Message);
        Assert.NotNull(result.Competition);
        Assert.Equal(5, result.SeasonsProcessed);
    }

    [Fact]
    public void CompetitionSyncResult_Failure_ShouldHaveNoCompetition()
    {
        // Arrange & Act
        var result = new CompetitionSyncResult(false, "Failed to fetch data", null, 0);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Failed to fetch data", result.Message);
        Assert.Null(result.Competition);
        Assert.Equal(0, result.SeasonsProcessed);
    }

    [Fact]
    public void TeamDetailsDto_ShouldBeReadonlyRecordStruct()
    {
        // Arrange
        var area = new AreaDto(2072, "England", "ENG", null);

        // Act
        var team = new TeamDetailsDto(
            57,
            "Arsenal FC",
            "Arsenal",
            "ARS",
            "https://crests.football-data.org/57.png",
            "75 Drayton Park London N5 1BU",
            "https://www.arsenal.com",
            1886,
            "Red / White",
            "Emirates Stadium",
            area);

        // Assert
        Assert.Equal(57, team.Id);
        Assert.Equal("Arsenal FC", team.Name);
        Assert.Equal("Arsenal", team.ShortName);
        Assert.Equal("ARS", team.Tla);
        Assert.Equal(1886, team.Founded);
        Assert.Equal("Red / White", team.ClubColors);
        Assert.Equal("Emirates Stadium", team.Venue);
        Assert.NotNull(team.Area);
        Assert.Equal("England", team.Area!.Value.Name);
    }
}

using DotMatchLens.Football.Models;

namespace DotMatchLens.Tests;

/// <summary>
/// Tests for Football DTOs.
/// </summary>
public sealed class FootballDtoTests
{
    [Fact]
    public void TeamDto_ShouldBeReadonlyRecordStruct()
    {
        // Arrange & Act
        var team = new TeamDto(
            Guid.NewGuid(),
            "Test Team",
            "Test Country",
            "Test League");

        // Assert
        Assert.Equal("Test Team", team.Name);
        Assert.Equal("Test Country", team.Country);
        Assert.Equal("Test League", team.League);
    }

    [Fact]
    public void TeamDto_ShouldSupportEquality()
    {
        // Arrange
        var id = Guid.NewGuid();
        var team1 = new TeamDto(id, "Team", "Country", "League");
        var team2 = new TeamDto(id, "Team", "Country", "League");

        // Assert
        Assert.Equal(team1, team2);
    }

    [Fact]
    public void PlayerDto_ShouldBeReadonlyRecordStruct()
    {
        // Arrange & Act
        var player = new PlayerDto(
            Guid.NewGuid(),
            "Test Player",
            "Forward",
            10,
            Guid.NewGuid(),
            "Team Name");

        // Assert
        Assert.Equal("Test Player", player.Name);
        Assert.Equal("Forward", player.Position);
        Assert.Equal(10, player.JerseyNumber);
        Assert.Equal("Team Name", player.TeamName);
    }

    [Fact]
    public void MatchDto_ShouldBeReadonlyRecordStruct()
    {
        // Arrange & Act
        var matchDate = DateTime.UtcNow;
        var match = new MatchDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Home Team",
            Guid.NewGuid(),
            "Away Team",
            matchDate,
            "Stadium",
            2,
            1,
            "Completed");

        // Assert
        Assert.Equal("Home Team", match.HomeTeamName);
        Assert.Equal("Away Team", match.AwayTeamName);
        Assert.Equal(2, match.HomeScore);
        Assert.Equal(1, match.AwayScore);
        Assert.Equal("Completed", match.Status);
    }

    [Fact]
    public void MatchEventDto_ShouldBeReadonlyRecordStruct()
    {
        // Arrange & Act
        var matchEvent = new MatchEventDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Goal",
            45,
            "Player Name",
            "Header from corner");

        // Assert
        Assert.Equal("Goal", matchEvent.EventType);
        Assert.Equal(45, matchEvent.Minute);
        Assert.Equal("Player Name", matchEvent.PlayerName);
        Assert.Equal("Header from corner", matchEvent.Description);
    }
}

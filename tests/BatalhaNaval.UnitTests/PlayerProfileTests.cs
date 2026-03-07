using BatalhaNaval.Domain.Entities;

namespace BatalhaNaval.UnitTests;

public class PlayerProfileTests
{
    [Fact]
    public void WinRate_WhenNoGamesPlayed_ShouldBeZero()
    {
        var profile = new PlayerProfile();
        Assert.Equal(0, profile.WinRate);
    }

    [Fact]
    public void WinRate_WhenGamesPlayed_ShouldBeCalculatedCorrectly()
    {
        var profile = new PlayerProfile { Wins = 1, Losses = 1 };
        Assert.Equal(0.5, profile.WinRate);
    }

    [Fact]
    public void HasMedal_WhenPlayerHasMedal_ShouldBeTrue()
    {
        var profile = new PlayerProfile();
        profile.AddMedal(1);
        Assert.True(profile.HasMedal(1));
    }

    [Fact]
    public void HasMedal_WhenPlayerDoesNotHaveMedal_ShouldBeFalse()
    {
        var profile = new PlayerProfile();
        Assert.False(profile.HasMedal(1));
    }

    [Fact]
    public void AddMedal_WhenPlayerDoesNotHaveMedal_ShouldAddMedal()
    {
        var profile = new PlayerProfile();
        profile.AddMedal(1);
        Assert.True(profile.HasMedal(1));
    }

    [Fact]
    public void AddMedal_WhenPlayerAlreadyHasMedal_ShouldNotAddMedalAgain()
    {
        var profile = new PlayerProfile();
        profile.AddMedal(1);
        profile.AddMedal(1);
        Assert.Single(profile.MedalIds);
    }

    [Fact]
    public void AddWin_ShouldIncrementWinsAndRankPoints()
    {
        var profile = new PlayerProfile { Wins = 0, RankPoints = 0 };
        profile.AddWin(10);
        Assert.Equal(1, profile.Wins);
        Assert.Equal(10, profile.RankPoints);
    }

    [Fact]
    public void AddWin_ShouldIncrementCurrentStreakAndMaxStreak()
    {
        var profile = new PlayerProfile { CurrentStreak = 0, MaxStreak = 0 };
        profile.AddWin(10);
        Assert.Equal(1, profile.CurrentStreak);
        Assert.Equal(1, profile.MaxStreak);
    }

    [Fact]
    public void AddLoss_ShouldIncrementLosses()
    {
        var profile = new PlayerProfile { Losses = 0 };
        profile.AddLoss();
        Assert.Equal(1, profile.Losses);
    }

    [Fact]
    public void AddLoss_ShouldResetCurrentStreak()
    {
        var profile = new PlayerProfile { CurrentStreak = 5 };
        profile.AddLoss();
        Assert.Equal(0, profile.CurrentStreak);
    }
}
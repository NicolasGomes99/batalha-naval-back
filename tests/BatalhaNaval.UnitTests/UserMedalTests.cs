using BatalhaNaval.Domain.Entities;

namespace BatalhaNaval.UnitTests;

public class UserMedalTests
{
    [Fact]
    public void Constructor_ValidArguments_ShouldInitializeProperties()
    {
        var userId = Guid.NewGuid();
        var medalId = 1;
        var userMedal = new UserMedal(userId, medalId);

        Assert.Equal(userId, userMedal.UserId);
        Assert.Equal(medalId, userMedal.MedalId);
        Assert.True(userMedal.EarnedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_EmptyUserId_ShouldThrowArgumentException()
    {
        var medalId = 1;
        Assert.Throws<ArgumentException>(() => new UserMedal(Guid.Empty, medalId));
    }

    [Fact]
    public void Constructor_InvalidMedalId_ShouldThrowArgumentException()
    {
        var userId = Guid.NewGuid();
        var medalId = 0; // Medalha inválida
        Assert.Throws<ArgumentException>(() => new UserMedal(userId, medalId));
    }
}
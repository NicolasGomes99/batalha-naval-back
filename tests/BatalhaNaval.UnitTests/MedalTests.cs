using BatalhaNaval.Domain.Entities;

namespace BatalhaNaval.UnitTests;

public class MedalTests
{
    [Fact]
    public void Constructor_ValidArguments_ShouldInitializeProperties()
    {
        var name = "Test Medal";
        var description = "A medal for testing";
        var code = "TEST_MEDAL";

        var medal = new Medal(name, description, code);

        Assert.Equal(name, medal.Name);
        Assert.Equal(description, medal.Description);
        Assert.Equal(code, medal.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_InvalidName_ShouldThrowArgumentException(string name)
    {
        var description = "A medal for testing";
        var code = "TEST_MEDAL";

        Assert.Throws<ArgumentException>(() => new Medal(name, description, code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_InvalidCode_ShouldThrowArgumentException(string code)
    {
        var name = "Test Medal";
        var description = "A medal for testing";

        Assert.Throws<ArgumentException>(() => new Medal(name, description, code));
    }
}
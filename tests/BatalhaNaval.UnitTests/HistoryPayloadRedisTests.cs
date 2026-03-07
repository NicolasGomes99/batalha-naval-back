using BatalhaNaval.Domain.Entities;
using BatalhaNaval.Domain.Enums;

namespace BatalhaNaval.UnitTests;

public class HistoryPayloadRedisTests
{
    [Fact]
    public void Properties_CanBeSetAndGet_Correctly()
    {
        var payload = new HistoryPayloadRedis
        {
            Coord = "A1",
            Result = "Hit",
            ShipIdx = 1,
            Direction = "North",
            Orientation = ShipOrientationRedis.HORIZONTAL,
            From = new List<ShipSegmentRedis> { new() },
            To = new List<ShipSegmentRedis> { new() }
        };

        Assert.Equal("A1", payload.Coord);
        Assert.Equal("Hit", payload.Result);
        Assert.Equal(1, payload.ShipIdx);
        Assert.Equal("North", payload.Direction);
        Assert.Equal(ShipOrientationRedis.HORIZONTAL, payload.Orientation);
        Assert.Single(payload.From);
        Assert.Single(payload.To);
    }

    [Fact]
    public void Properties_CanBeNull_Correctly()
    {
        var payload = new HistoryPayloadRedis();

        Assert.Null(payload.Coord);
        Assert.Null(payload.Result);
        Assert.Null(payload.ShipIdx);
        Assert.Null(payload.Direction);
        Assert.Null(payload.Orientation);
        Assert.Null(payload.From);
        Assert.Null(payload.To);
    }
}
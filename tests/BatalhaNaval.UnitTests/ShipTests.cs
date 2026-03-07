using BatalhaNaval.Domain.Entities;
using BatalhaNaval.Domain.Enums;
using BatalhaNaval.Domain.ValueObjects;

namespace BatalhaNaval.UnitTests;

public class ShipTests
{
    [Fact]
    public void Constructor_WithSizeAndCoordinatesMismatch_ShouldThrowArgumentException()
    {
        var coordinates = new List<Coordinate> { new(0, 0), new(0, 1) };
        Assert.Throws<ArgumentException>(() => new Ship("Test Ship", 3, coordinates, ShipOrientation.Horizontal));
    }

    [Fact]
    public void IsSunk_WhenAllCoordinatesAreHit_ShouldBeTrue()
    {
        var coordinates = new List<Coordinate> { new(0, 0, true), new(0, 1, true) };
        var ship = new Ship("Test Ship", 2, coordinates, ShipOrientation.Horizontal);
        Assert.True(ship.IsSunk);
    }

    [Fact]
    public void IsSunk_WhenNotAllCoordinatesAreHit_ShouldBeFalse()
    {
        var coordinates = new List<Coordinate> { new(0, 0, true), new(0, 1) };
        var ship = new Ship("Test Ship", 2, coordinates, ShipOrientation.Horizontal);
        Assert.False(ship.IsSunk);
    }

    [Fact]
    public void HasBeenHit_WhenAnyCoordinateIsHit_ShouldBeTrue()
    {
        var coordinates = new List<Coordinate> { new(0, 0, true), new(0, 1) };
        var ship = new Ship("Test Ship", 2, coordinates, ShipOrientation.Horizontal);
        Assert.True(ship.HasBeenHit);
    }

    [Fact]
    public void HasBeenHit_WhenNoCoordinateIsHit_ShouldBeFalse()
    {
        var coordinates = new List<Coordinate> { new(0, 0), new(0, 1) };
        var ship = new Ship("Test Ship", 2, coordinates, ShipOrientation.Horizontal);
        Assert.False(ship.HasBeenHit);
    }

    [Fact]
    public void PredictMovement_WhenShipIsHit_ShouldThrowInvalidOperationException()
    {
        var coordinates = new List<Coordinate> { new(0, 0, true), new(0, 1) };
        var ship = new Ship("Test Ship", 2, coordinates, ShipOrientation.Horizontal);
        Assert.Throws<InvalidOperationException>(() => ship.PredictMovement(MoveDirection.East));
    }

    [Fact]
    public void PredictMovement_ForHorizontalShipMovingVertically_ShouldThrowInvalidOperationException()
    {
        var coordinates = new List<Coordinate> { new(0, 0), new(1, 0) };
        var ship = new Ship("Test Ship", 2, coordinates, ShipOrientation.Horizontal);
        Assert.Throws<InvalidOperationException>(() => ship.PredictMovement(MoveDirection.North));
    }

    [Fact]
    public void PredictMovement_ForVerticalShipMovingHorizontally_ShouldThrowInvalidOperationException()
    {
        var coordinates = new List<Coordinate> { new(0, 0), new(0, 1) };
        var ship = new Ship("Test Ship", 2, coordinates, ShipOrientation.Vertical);
        Assert.Throws<InvalidOperationException>(() => ship.PredictMovement(MoveDirection.East));
    }

    [Theory]
    [InlineData(MoveDirection.North, 0, -1)]
    [InlineData(MoveDirection.South, 0, 1)]
    [InlineData(MoveDirection.East, 1, 0)]
    [InlineData(MoveDirection.West, -1, 0)]
    public void PredictMovement_ShouldReturnCorrectNewCoordinates(MoveDirection direction, int deltaX, int deltaY)
    {
        var coordinates = new List<Coordinate> { new(1, 1) };
        var ship = new Ship("Test Ship", 1, coordinates, ShipOrientation.Horizontal);
        var newCoordinates = ship.PredictMovement(direction);
        Assert.Equal(new Coordinate(1 + deltaX, 1 + deltaY), newCoordinates.First());
    }

    [Fact]
    public void ConfirmMovement_WithInvalidNumberOfCoordinates_ShouldThrowInvalidOperationException()
    {
        var ship = new Ship("Test Ship", 2, new List<Coordinate> { new(0, 0), new(0, 1) },
            ShipOrientation.Horizontal);
        var newCoordinates = new List<Coordinate> { new(1, 1) };
        Assert.Throws<InvalidOperationException>(() => ship.ConfirmMovement(newCoordinates));
    }

    [Fact]
    public void ConfirmMovement_WithValidCoordinates_ShouldUpdateShipCoordinates()
    {
        var ship = new Ship("Test Ship", 2, new List<Coordinate> { new(0, 0), new(0, 1) },
            ShipOrientation.Horizontal);
        var newCoordinates = new List<Coordinate> { new(1, 0), new(1, 1) };
        ship.ConfirmMovement(newCoordinates);
        Assert.Equal(newCoordinates, ship.Coordinates);
    }

    [Fact]
    public void UpdateDamage_WithInvalidNumberOfCoordinates_ShouldThrowArgumentException()
    {
        var ship = new Ship("Test Ship", 2, new List<Coordinate> { new(0, 0), new(0, 1) },
            ShipOrientation.Horizontal);
        var updatedCoordinates = new List<Coordinate> { new(0, 0, true) };
        Assert.Throws<ArgumentException>(() => ship.UpdateDamage(updatedCoordinates));
    }

    [Fact]
    public void UpdateDamage_WithValidCoordinates_ShouldUpdateShipCoordinates()
    {
        var ship = new Ship("Test Ship", 2, new List<Coordinate> { new(0, 0), new(0, 1) },
            ShipOrientation.Horizontal);
        var updatedCoordinates = new List<Coordinate> { new(0, 0, true), new(0, 1) };
        ship.UpdateDamage(updatedCoordinates);
        Assert.Equal(updatedCoordinates, ship.Coordinates);
    }
}
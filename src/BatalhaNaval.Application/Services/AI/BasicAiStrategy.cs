using BatalhaNaval.Domain.Entities;
using BatalhaNaval.Domain.Enums;
using BatalhaNaval.Domain.Interfaces;
using BatalhaNaval.Domain.ValueObjects;

namespace BatalhaNaval.Application.Services.AI;

public class BasicAiStrategy : IAiStrategy
{
    private readonly Random _random = new();

    public Coordinate ChooseTarget(Board enemyBoard)
    {
        var validTargets = new List<Coordinate>();
        for (var x = 0; x < Board.Size; x++)
        for (var y = 0; y < Board.Size; y++)
        {
            var cell = enemyBoard.Cells[x][y];
            if (cell != CellState.Hit && cell != CellState.Missed) validTargets.Add(new Coordinate(x, y));
        }

        if (validTargets.Count == 0) return new Coordinate(0, 0);
        return validTargets[_random.Next(validTargets.Count)];
    }
}
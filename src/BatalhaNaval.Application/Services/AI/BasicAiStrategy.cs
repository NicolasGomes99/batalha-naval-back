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
        // IA Básica: Apenas chuta coordenadas que ainda não foram atingidas
        // (Água ou Navio oculto, desde que não seja Hit ou Missed)
        var validTargets = new List<Coordinate>();

        for (int x = 0; x < Board.Size; x++)
        {
            for (int y = 0; y < Board.Size; y++)
            {
                var cell = enemyBoard.Cells[x, y];
                if (cell != CellState.Hit && cell != CellState.Missed)
                {
                    validTargets.Add(new Coordinate(x, y));
                }
            }
        }

        if (validTargets.Count == 0) return new Coordinate(0, 0); // Fallback (não deve ocorrer)

        return validTargets[_random.Next(validTargets.Count)];
    }
}
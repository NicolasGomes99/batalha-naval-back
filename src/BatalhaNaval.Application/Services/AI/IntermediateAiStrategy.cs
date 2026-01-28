using BatalhaNaval.Domain.Entities;
using BatalhaNaval.Domain.Enums;
using BatalhaNaval.Domain.Interfaces;
using BatalhaNaval.Domain.ValueObjects;

namespace BatalhaNaval.Application.Services.AI;

public class IntermediateAiStrategy : IAiStrategy
{
    private readonly Random _random = new();

    public Coordinate ChooseTarget(Board enemyBoard)
    {
        // 1. Modo "Target": Procura por células HIT que pertencem a navios NÃO afundados.
        // Isso significa que há um navio ferido precisando ser finalizado.
        var damagedShips = enemyBoard.Ships.Where(s => s.HasBeenHit && !s.IsSunk).ToList();

        foreach (var ship in damagedShips)
        {
            // Pega as coordenadas atingidas deste navio
            var hitCoords = ship.Coordinates.Where(c => c.IsHit).ToList();

            // Tenta atirar nos vizinhos (Norte, Sul, Leste, Oeste) de cada acerto
            foreach (var hit in hitCoords)
            {
                var neighbors = GetValidNeighbors(hit, enemyBoard);
                if (neighbors.Any()) return neighbors[_random.Next(neighbors.Count)];
            }
        }

        // 2. Modo "Hunt": Se não há navios feridos para focar, atira aleatoriamente (como a Básica)
        // Otimização: Paridade (tabuleiro de xadrez) poderia ser aplicada aqui, mas vamos manter simples.
        return new BasicAiStrategy().ChooseTarget(enemyBoard);
    }

    private List<Coordinate> GetValidNeighbors(Coordinate c, Board board)
    {
        var deltas = new List<(int dx, int dy)> { (0, -1), (0, 1), (-1, 0), (1, 0) };
        var valid = new List<Coordinate>();

        foreach (var (dx, dy) in deltas)
        {
            var nx = c.X + dx;
            var ny = c.Y + dy;

            // Verifica limites
            if (nx >= 0 && nx < Board.Size && ny >= 0 && ny < Board.Size)
            {
                // Verifica se já não atirou lá
                var cell = board.Cells[nx][ny];
                if (cell != CellState.Hit && cell != CellState.Missed) valid.Add(new Coordinate(nx, ny));
            }
        }

        return valid;
    }
}
using BatalhaNaval.Domain.Enums;
using BatalhaNaval.Domain.ValueObjects;

namespace BatalhaNaval.Domain.Entities;

public class Board
{
    public const int Size = 10;

    public Board()
    {
        // Inicializa a grade 10x10 com Água
        Cells = new List<List<CellState>>();
        for (var x = 0; x < Size; x++)
        {
            var row = new List<CellState>();
            for (var y = 0; y < Size; y++) row.Add(CellState.Water);
            Cells.Add(row);
        }
    }

    public List<Ship> Ships { get; } = new();

    // Isso é amigável para JSON e EF Core
    public List<List<CellState>> Cells { get; }

    public void AddShip(Ship ship)
    {
        ValidateCoordinatesOrThrow(ship.Coordinates, ship.Id);
        Ships.Add(ship);

        foreach (var coord in ship.Coordinates)
            // MUDANÇA DE SINTAXE: [x][y]
            Cells[coord.X][coord.Y] = CellState.Ship;
    }

    public void MoveShip(Guid shipId, MoveDirection direction)
    {
        var ship = Ships.FirstOrDefault(s => s.Id == shipId);
        if (ship == null)
            throw new KeyNotFoundException("Navio não encontrado neste tabuleiro.");

        var proposedCoordinates = ship.PredictMovement(direction);
        ValidateCoordinatesOrThrow(proposedCoordinates, ship.Id);

        // Limpa visual
        foreach (var coord in ship.Coordinates) Cells[coord.X][coord.Y] = CellState.Water;

        ship.ConfirmMovement(proposedCoordinates);

        // Atualiza visual
        foreach (var coord in ship.Coordinates) Cells[coord.X][coord.Y] = CellState.Ship;
    }

    private void ValidateCoordinatesOrThrow(List<Coordinate> coords, Guid ignoreShipId)
    {
        foreach (var coord in coords)
        {
            if (!coord.IsWithinBounds(Size))
                throw new InvalidOperationException("Coordenada fora dos limites do tabuleiro.");

            var isOccupied = Ships.Any(otherShip =>
                otherShip.Id != ignoreShipId &&
                otherShip.Coordinates.Any(c => c.X == coord.X && c.Y == coord.Y));

            if (isOccupied)
                throw new InvalidOperationException("Coordenada já ocupada por outro navio.");
        }
    }

    public bool ReceiveShot(int x, int y)
    {
        // Validação de segurança para acesso a lista
        if (x < 0 || x >= Size || y < 0 || y >= Size) return false;

        // MUDANÇA DE SINTAXE: [x][y]
        if (Cells[x][y] == CellState.Hit || Cells[x][y] == CellState.Missed)
            return false;

        var ship = Ships.FirstOrDefault(s => s.Coordinates.Any(c => c.X == x && c.Y == y));

        if (ship != null)
        {
            var coord = ship.Coordinates.First(c => c.X == x && c.Y == y);
            var newCoords = new List<Coordinate>(ship.Coordinates);
            var index = newCoords.IndexOf(coord);
            newCoords[index] = coord with { IsHit = true };

            ship.UpdateDamage(newCoords);

            Cells[x][y] = CellState.Hit;
            return true;
        }

        Cells[x][y] = CellState.Missed;
        return false;
    }

    public bool AllShipsSunk()
    {
        return Ships.Count > 0 && Ships.All(s => s.IsSunk);
    }
}
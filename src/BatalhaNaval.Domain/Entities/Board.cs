using BatalhaNaval.Domain.Enums;
using BatalhaNaval.Domain.Exceptions;
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

    // Representação visual do tabuleiro
    public List<List<CellState>> Cells { get; }

    public void AddShip(Ship ship)
    {
        // Validação inicial de posicionamento (Setup)
        ValidateCoordinatesOrThrow(ship.Coordinates, ship.Id);
        Ships.Add(ship);

        foreach (var coord in ship.Coordinates)
            Cells[coord.X][coord.Y] = CellState.Ship;
    }

    public void MoveShip(Guid shipId, MoveDirection direction)
    {
        var ship = Ships.FirstOrDefault(s => s.Id == shipId);
        if (ship == null)
            throw new KeyNotFoundException("Navio não encontrado neste tabuleiro.");

        if (ship.IsSunk)
            throw new InvalidOperationException("Navios afundados não podem se mover.");
        
        // 1. VALIDAÇÃO FÍSICA (Respeitar a Orientação)
        // Navios maiores que 1 célula (Submarino é isento) não podem andar de lado (Strafing).
        if (ship.Size > 1) 
        {
            var isVertical = ship.Orientation == ShipOrientation.Vertical;
            var isHorizontal = ship.Orientation == ShipOrientation.Horizontal;

            // Se Vertical, não pode ir Esquerda/Direita
            if (isVertical && (direction == MoveDirection.West || direction == MoveDirection.East))
            {
                throw new InvalidOperationException($"O navio '{ship.Name}' (Vertical) só pode se mover para Cima ou Baixo.");
            }

            // Se Horizontal, não pode ir Cima/Baixo
            if (isHorizontal && (direction == MoveDirection.North || direction == MoveDirection.South))
            {
                throw new InvalidOperationException($"O navio '{ship.Name}' (Horizontal) só pode se mover para Esquerda ou Direita.");
            }
        }

        // Previsão das novas coordenadas
        var proposedCoordinates = ship.PredictMovement(direction);
        
        // 2. VALIDAÇÃO DE COLISÃO (Limites, Outros Navios e Tiros)
        ValidateCoordinatesOrThrow(proposedCoordinates, ship.Id);

        // Se passou nas validações, atualiza o tabuleiro visualmente
        
        // A. Limpa a posição antiga (pinta de água)
        foreach (var coord in ship.Coordinates) 
            Cells[coord.X][coord.Y] = CellState.Water;

        // B. Atualiza a entidade Navio
        ship.ConfirmMovement(proposedCoordinates);

        // C. Pinta a nova posição (pinta de Navio)
        foreach (var coord in ship.Coordinates) 
            Cells[coord.X][coord.Y] = CellState.Ship;
    }

    private void ValidateCoordinatesOrThrow(List<Coordinate> coords, Guid ignoreShipId)
    {
        foreach (var coord in coords)
        {
            // Validação A: Limites do Mapa
            if (!coord.IsWithinBounds(Size))
                throw new InvalidOperationException("O movimento faria o navio sair dos limites do tabuleiro.");

            // Validação B: Colisão com Objetos do Cenário (Tiros/Destroços)
            // Aqui garantimos que o navio não "atropela" um tiro
            var currentCellState = Cells[coord.X][coord.Y];
            if (currentCellState == CellState.Hit || currentCellState == CellState.Missed)
            {
                throw new InvalidOperationException("O navio não pode se mover para uma área atingida por disparos.");
            }

            // Validação C: Colisão com Outros Navios
            // Verifica se a coordenada bate em algum navio (ignorando o próprio navio que está se movendo)
            var isOccupiedByAnotherShip = Ships.Any(otherShip =>
                otherShip.Id != ignoreShipId &&
                otherShip.Coordinates.Any(c => c.X == coord.X && c.Y == coord.Y));

            if (isOccupiedByAnotherShip)
                throw new InvalidOperationException("O movimento causaria colisão com outro navio.");
        }
    }

    public bool ReceiveShot(int x, int y)
    {
        // 1. Validação de Limites
        if (x < 0 || x >= Size || y < 0 || y >= Size)
        {
            // Sugestão: InvalidCoordinateException é ótima se você tiver criado ela. 
            // Se não, use ArgumentOutOfRangeException ou InvalidOperationException.
            throw new InvalidOperationException($"Coordenada ({x}, {y}) está fora dos limites do tabuleiro.");
        }

        // 2. Validação de Tiro Repetido (A "Trava")
        // Se a célula não for Água e não for Navio "Virgem", então já foi atingida.
        var currentCell = Cells[x][y];
        if (currentCell == CellState.Hit || currentCell == CellState.Missed)
        {
            throw new InvalidOperationException($"A posição ({x}, {y}) já foi alvejada previamente. Escolha outra.");
        }

        // 3. Verifica se acertou Navio
        // Nota: O método Any() é seguro, mas certifique-se que Ships não seja nulo (inicializado no construtor)
        var ship = Ships.FirstOrDefault(s => s.Coordinates.Any(c => c.X == x && c.Y == y));

        if (ship != null)
        {
            var coord = ship.Coordinates.First(c => c.X == x && c.Y == y);
        
            // Atualiza o estado de dano do navio
            // (Isso é importante para checar IsSunk depois)
            var newCoords = new List<Coordinate>(ship.Coordinates);
            var index = newCoords.IndexOf(coord);
            newCoords[index] = coord with { IsHit = true };
            ship.UpdateDamage(newCoords);

            // Atualiza o visual do tabuleiro
            Cells[x][y] = CellState.Hit;
            return true; // Acertou
        }

        // 4. Se não acertou nada (Água)
        Cells[x][y] = CellState.Missed;
        return false; // Errou
    }

    public bool AllShipsSunk()
    {
        return Ships.Count > 0 && Ships.All(s => s.IsSunk);
    }
}
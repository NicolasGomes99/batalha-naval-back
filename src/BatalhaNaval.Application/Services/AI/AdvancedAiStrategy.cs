using BatalhaNaval.Domain.Entities;
using BatalhaNaval.Domain.Enums;
using BatalhaNaval.Domain.Interfaces;
using BatalhaNaval.Domain.ValueObjects;

namespace BatalhaNaval.Application.Services.AI;

public class AdvancedAiStrategy : IAiStrategy
{
    private readonly Random _random = new();

    public Coordinate ChooseTarget(Board enemyBoard)
    {
        // Se houver um navio ferido (modo Target), a lógica Intermediária já é ótima.
        // A Avançada brilha no modo "Hunt" (quando não sabe onde atirar).
        if (enemyBoard.Ships.Any(s => s.HasBeenHit && !s.IsSunk))
            return new IntermediateAiStrategy().ChooseTarget(enemyBoard);

        // Mapa de calor: Matriz de probabilidade
        var heatMap = new int[Board.Size, Board.Size];

        // Navios que AINDA estão vivos (não afundados)
        var remainingShips = enemyBoard.Ships.Where(s => !s.IsSunk).ToList();

        // Para cada navio vivo, simulamos todas as posições possíveis no tabuleiro atual
        foreach (var ship in remainingShips) SimulateShipPositions(ship.Size, enemyBoard, heatMap);

        // Encontra a célula com maior pontuação no mapa de calor
        return GetBestCoordinate(heatMap, enemyBoard);
    }

    private void SimulateShipPositions(int size, Board board, int[,] heatMap)
    {
        // Tenta encaixar horizontalmente
        for (var y = 0; y < Board.Size; y++)
        for (var x = 0; x <= Board.Size - size; x++)
            if (CanFit(x, y, size, true, board))
                // Incrementa probabilidade nessas células
                for (var k = 0; k < size; k++)
                    heatMap[x + k, y]++;

        // Tenta encaixar verticalmente
        for (var x = 0; x < Board.Size; x++)
        for (var y = 0; y <= Board.Size - size; y++)
            if (CanFit(x, y, size, false, board))
                for (var k = 0; k < size; k++)
                    heatMap[x, y + k]++;
    }

    private bool CanFit(int x, int y, int size, bool isHorizontal, Board board)
    {
        for (var k = 0; k < size; k++)
        {
            var cx = isHorizontal ? x + k : x;
            var cy = isHorizontal ? y : y + k;

            // Se encontrar um tiro na água (Missed) ou um navio já afundado (Hit), não cabe aqui.
            // Nota: Se for Hit mas o navio não afundou, tecnicamente *poderia* ser parte dele,
            // mas simplificamos assumindo que estamos no modo "Hunt" (procurando no escuro).
            if (board.Cells[cx][cy] == CellState.Missed || board.Cells[cx][cy] == CellState.Hit)
                return false;
        }

        return true;
    }

    private Coordinate GetBestCoordinate(int[,] heatMap, Board board)
    {
        var maxScore = -1;
        List<Coordinate> bestCandidates = new();

        for (var x = 0; x < Board.Size; x++)
        for (var y = 0; y < Board.Size; y++)
        {
            // Só considera células válidas para tiro
            if (board.Cells[x][y] == CellState.Hit || board.Cells[x][y] == CellState.Missed) continue;

            if (heatMap[x, y] > maxScore)
            {
                maxScore = heatMap[x, y];
                bestCandidates.Clear();
                bestCandidates.Add(new Coordinate(x, y));
            }
            else if (heatMap[x, y] == maxScore)
            {
                bestCandidates.Add(new Coordinate(x, y));
            }
        }

        // Se não houver candidatos (improvável), usa aleatório
        if (bestCandidates.Count == 0) return new BasicAiStrategy().ChooseTarget(board);

        return bestCandidates[_random.Next(bestCandidates.Count)];
    }
}
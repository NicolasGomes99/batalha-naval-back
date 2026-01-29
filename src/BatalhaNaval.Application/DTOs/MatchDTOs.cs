using BatalhaNaval.Domain.Enums;

namespace BatalhaNaval.Application.DTOs;

// Entrada para criar partida
public record StartMatchInput(
    GameMode Mode,
    Difficulty? AiDifficulty = null,
    Guid? OpponentId = null
);

// Entrada para realizar um tiro
public record ShootInput(Guid MatchId, int X, int Y);

// Entrada para mover navio (Modo Dinâmico)
public record MoveShipInput(Guid MatchId, Guid PlayerId, Guid ShipId, MoveDirection Direction);

// Entrada para posicionar navios (Setup)
public record PlaceShipsInput(Guid MatchId, List<ShipPlacementDto> Ships);

public record ShipPlacementDto(string Name, int Size, int StartX, int StartY, ShipOrientation Orientation);

// Saída de Status de Jogada (Retorno para o BFF)
public record TurnResultDto(
    bool IsHit,
    bool IsSunk,
    bool IsGameOver,
    Guid? WinnerId,
    string Message
);
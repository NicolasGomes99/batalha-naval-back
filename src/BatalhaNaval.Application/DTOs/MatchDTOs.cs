using BatalhaNaval.Domain.Enums;

namespace BatalhaNaval.Application.DTOs;

// Entrada para criar partida
public record StartMatchInput(
    Guid PlayerId,
    GameMode Mode,
    Difficulty? AiDifficulty = null,
    Guid? OpponentId = null
);

// Entrada para realizar um tiro
public record ShootInput(Guid MatchId, Guid PlayerId, int X, int Y);

// Entrada para mover navio (Modo Dinâmico)
public record MoveShipInput(Guid MatchId, Guid PlayerId, Guid ShipId, MoveDirection Direction);

// Entrada para posicionar navios (Setup)
public record PlaceShipsInput(Guid MatchId, Guid PlayerId, List<ShipPlacementDto> Ships);

public record ShipPlacementDto(string Name, int Size, int StartX, int StartY, ShipOrientation Orientation);

// Saída de Status de Jogada (Retorno para o BFF)
public record TurnResultDto(
    bool IsHit,
    bool IsSunk,
    bool IsGameOver,
    Guid? WinnerId,
    string Message
);
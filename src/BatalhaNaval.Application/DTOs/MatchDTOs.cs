using BatalhaNaval.Domain.Enums;

namespace BatalhaNaval.Application.DTOs;

// === DTOs de ENTRADA (Inputs) ===

// Entrada para criar partida
public record StartMatchInput(
    GameMode Mode,
    Difficulty? AiDifficulty = null,
    Guid? OpponentId = null
);

// Entrada para realizar um tiro
public record ShootInput(Guid MatchId, int X, int Y);

// Entrada para mover navio (Modo Dinâmico)
public record MoveShipInput(Guid MatchId, Guid ShipId, MoveDirection Direction);

// Entrada para posicionar navios (Setup)
public record PlaceShipsInput(Guid MatchId, List<ShipPlacementDto> Ships);

public record ShipPlacementDto(string Name, int Size, int StartX, int StartY, ShipOrientation Orientation);

// === DTOs de SAÍDA (Outputs / View Models) ===

// Retorno simplificado para ações de jogo (Tiro/Movimento)
public record TurnResultDto(
    bool IsHit,
    bool IsSunk,
    bool IsGameOver,
    Guid? WinnerId,
    string Message
);

// Retorno COMPLETO do Estado da Partida (com Fog of War)
public record MatchGameStateDto(
    Guid MatchId,
    MatchStatus Status,
    Guid CurrentTurnPlayerId,
    bool IsMyTurn,              // Facilita pro Frontend saber se habilita os controles
    Guid? WinnerId,
    BoardStateDto MyBoard,      // Tabuleiro do jogador (vê tudo)
    BoardStateDto OpponentBoard,// Tabuleiro do oponente (mascarado)
    MatchStatsDto Stats
);

public record BoardStateDto(
    List<List<CellState>> Grid, // A matriz visual 10x10
    List<ShipDto>? Ships        // Lista de navios (Null ou vazia para o oponente)
);

public record ShipDto(
    Guid Id, 
    string Name, 
    int Size, 
    bool IsSunk, 
    ShipOrientation Orientation,
    List<CoordinateDto> Coordinates
);

public record CoordinateDto(int X, int Y, bool IsHit);

public record MatchStatsDto(
    int MyHits, 
    int MyStreak, 
    int OpponentHits, 
    int OpponentStreak
);
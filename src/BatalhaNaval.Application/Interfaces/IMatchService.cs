using BatalhaNaval.Application.DTOs;

namespace BatalhaNaval.Application.Interfaces;

public interface IMatchService
{
    Task<Guid> StartMatchAsync(StartMatchInput input);

    Task SetupShipsAsync(PlaceShipsInput input);

    Task<TurnResultDto> ExecutePlayerShotAsync(ShootInput input);

    Task ExecutePlayerMoveAsync(MoveShipInput input); // Modo Dinâmico
    // O turno da IA pode ser disparado automaticamente após o turno do jogador
}
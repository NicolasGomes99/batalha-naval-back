using BatalhaNaval.API.Extensions;
using BatalhaNaval.Application.DTOs;
using BatalhaNaval.Application.Interfaces;
using BatalhaNaval.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace batalha_naval_back.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Authorize]
public class MatchController : ControllerBase
{
    private readonly ILogger<MatchController> _logger;
    private readonly IMatchService _matchService;

    public MatchController(IMatchService matchService, ILogger<MatchController> logger)
    {
        _matchService = matchService;
        _logger = logger;
    }

    /// <summary>
    ///     Inicia uma nova partida (PvP ou PvE)
    /// </summary>
    /// <remarks>
    ///     Inicia uma nova partida (PvP ou PvE).
    /// </remarks>
    /// <response code="201">Partida iniciada com sucesso.</response>
    /// <response code="400">Dados inválidos para iniciar a partida.</response>
    /// <response code="404">Recurso não encontrado.</response>
    /// <response code="409">Conflito ao iniciar partida (usuário já em jogo ou oponente ocupado).</response>
    /// <response code="500">Erro interno ao criar partida.</response>
    [HttpPost(Name = "PostStartMatch")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartMatch([FromBody] StartMatchInput input)
    {
        try
        {
            var playerId = User.GetUserId();
            var matchId = await _matchService.StartMatchAsync(input, playerId);
            return CreatedAtAction(
                nameof(StartMatch),
                new { id = matchId },
                new { matchId }
            );
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Recurso não encontrado",
                detail: ex.Message
            );
        }
        catch (ArgumentException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Dados inválidos",
                detail: ex.Message
            );
        }
        catch (UserHasActiveMatchException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Partida em andamento",
                detail: "Você já possui uma partida ativa. Retome-a utilizando o ID fornecido.",
                extensions: new Dictionary<string, object?> { { "activeMatchId", ex.MatchId } }
            );
        }
        catch (OpponentBusyException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Oponente indisponível",
                detail: ex.Message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar partida.");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Erro interno",
                detail: "Ocorreu um erro inesperado ao criar a partida. Tente novamente."
            );
        }
    }

    /// <summary>
    ///     Posiciona os navios no tabuleiro (Fase de Setup)
    /// </summary>
    /// <remarks>
    ///     Posiciona os navios no tabuleiro durante a fase de setup.
    /// </remarks>
    /// <response code="200">Navios posicionados com sucesso.</response>
    /// <response code="400">Dados inválidos para posicionar os navios.</response>
    /// <response code="404">Partida não encontrada.</response>
    [HttpPost("setup", Name = "PostSetupShips")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetupShips([FromBody] PlaceShipsInput input)
    {
        try
        {
            var playerId = User.GetUserId();
            await _matchService.SetupShipsAsync(input, playerId);
            return Ok(new { message = "Navios posicionados com sucesso. Aguardando início." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao posicionar navios.");
            return StatusCode(500, new { error = "Erro interno ao processar setup." });
        }
    }

    /// <summary>
    ///     Executa um tiro (Ação principal do jogo)
    /// </summary>
    /// <remarks>
    ///     Executa um tiro durante o jogo.
    /// </remarks>
    /// <response code="200">Tiro executado com sucesso.</response>
    /// <response code="400">Dados inválidos para executar o tiro.</response>
    /// <response code="404">Partida não encontrada.</response>
    [HttpPost("shot", Name = "PostExecuteShot")]
    [ProducesResponseType(typeof(TurnResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExecuteShot([FromBody] ShootInput input)
    {
        try
        {
            var playerId = User.GetUserId();
            var result = await _matchService.ExecutePlayerShotAsync(input, playerId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) // Turno errado, jogo finalizado, etc.
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (TimeoutException ex) // Tempo esgotado
        {
            return StatusCode(408, new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Move um navio (Apenas Modo Dinâmico)
    /// </summary>
    /// <remarks>
    ///     Move um navio durante o jogo no modo dinâmico.
    /// </remarks>
    /// <response code="200">Navio movido com sucesso.</response>
    /// <response code="400">Dados inválidos para mover o navio.</response>
    [HttpPost("move", Name = "PostMoveShip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MoveShip([FromBody] MoveShipInput input)
    {
        try
        {
            await _matchService.ExecutePlayerMoveAsync(input);
            return Ok(new { message = "Navio movido com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    ///     Cancela uma partida existente.
    /// </summary>
    /// <remarks>
    ///     Se a partida estiver em 'Setup', ela é excluída sem penalidades.
    ///     Se estiver 'InProgress', conta como derrota para quem cancelou.
    /// </remarks>
    /// <response code="204">Partida cancelada com sucesso.</response>
    /// <response code="401">Token inválido ou ausente.</response>
    /// <response code="403">Jogador não é dono da partida.</response>
    /// <response code="404">Partida não encontrada.</response>
    /// <response code="409">Partida já finalizada.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelMatch(Guid id)
    {
        var playerId = User.GetUserId();
        
        await _matchService.CancelMatchAsync(id, playerId);

        return NoContent();
    }
}
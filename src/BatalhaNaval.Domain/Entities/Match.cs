using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using BatalhaNaval.Domain.Enums;

namespace BatalhaNaval.Domain.Entities;

public class Match
{
    private bool _player1Ready;
    private bool _player2Ready;

    // Estatísticas de Jogo
    
    [Column("player1_hits")]
    public int Player1Hits { get; private set; }
    
    [Column("player2_hits")]
    public int Player2Hits { get; private set; }
    
    // NOVA PROPRIEDADE: Controle de Streak (Acertos Consecutivos)
    [Column("player1_consecutive_hits")]
    public int Player1ConsecutiveHits { get; private set; }
    
    [Column("player2_consecutive_hits")]
    public int Player2ConsecutiveHits { get; private set; }
    
    [Column("has_moved_this_turn")]
    public bool HasMovedThisTurn { get; private set; }

    public Match(Guid player1Id, GameMode mode, Difficulty? aiDifficulty = null, Guid? player2Id = null)
    {
        Id = Guid.NewGuid();
        Player1Id = player1Id;
        Player2Id = player2Id;
        Mode = mode;
        AiDifficulty = aiDifficulty;
        Status = MatchStatus.Setup;

        Player1Board = new Board();
        Player2Board = new Board();

        // A decisão de quem começa jogando pra valer fica no StartGame.
        CurrentTurnPlayerId = player1Id;
    }

    [Description("Identificador único da partida")]
    public Guid Id { get; private set; }

    [Description("Identificador único do jogador 1")]
    public Guid Player1Id { get; }

    [Description("Identificador único do jogador 2")]
    public Guid? Player2Id { get; }

    [Description("Tabuleiro do jogador 1")]
    public Board Player1Board { get; }

    [Description("Tabuleiro do jogador 2")]
    public Board Player2Board { get; }

    [Description("Modo de jogo")] 
    public GameMode Mode { get; }

    [Description("Dificuldade da IA, se aplicável")]
    public Difficulty? AiDifficulty { get; private set; }

    [Description("Status atual da partida")]
    [Column("status")]
    public MatchStatus Status { get; set; }

    [Description("Indica se a partida está finalizada")]
    [Column("is_finished")]
    public bool IsFinished => Status == MatchStatus.Finished;

    [Description("Hora de encerramento da partida")]
    [Column("finished_at")]
    public DateTime? FinishedAt { get; set; }

    [Description("Identificador único do jogador atual")]
    public Guid CurrentTurnPlayerId { get; private set; }

    [Description("Identificador único do vencedor")]
    public Guid? WinnerId { get; set; }

    [Description("Data e hora de início da partida")]
    public DateTime StartedAt { get; private set; } // Quando o status muda para InProgress

    [Description("Data e hora do último movimento")]
    public DateTime LastMoveAt { get; private set; }

    // Método chamado quando o jogador termina de posicionar navios
    public void SetPlayerReady(Guid playerId)
    {
        if (Status != MatchStatus.Setup) return;

        if (playerId == Player1Id) _player1Ready = true;
        else if (playerId == Player2Id) _player2Ready = true;

        // Verifica se pode iniciar o jogo
        var isAiGame = Player2Id == null;
        if (_player1Ready && (_player2Ready || isAiGame)) StartGame();
    }

    private void StartGame()
    {
        Status = MatchStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        LastMoveAt = DateTime.UtcNow;

        var random = new Random();
        // Se 0, começa P1. Se 1, começa P2 (ou IA representada por Guid.Empty)
        var starter = random.Next(2);

        if (starter == 0)
        {
            CurrentTurnPlayerId = Player1Id;
        }
        else
        {
            // Se P2 for null, é IA (Guid.Empty)
            CurrentTurnPlayerId = Player2Id ?? Guid.Empty;
        }
        // NOVO: Reseta flag de movimento para o primeiro turno
        HasMovedThisTurn = false;
    }

    // Ação 1: Atirar
    public bool ExecuteShot(Guid playerId, int x, int y)
    {
        ValidateTurn(playerId);

        var targetBoard = playerId == Player1Id ? Player2Board : Player1Board;

        // O Board deve lançar exceção se o tiro for repetido
        bool isHit = targetBoard.ReceiveShot(x, y);

        // 2. Atualização de Estatísticas (Total e Consecutivo)
        if (isHit)
        {
            if (playerId == Player1Id)
            {
                Player1Hits++;
                Player1ConsecutiveHits++;
            }
            else
            {
                Player2Hits++;
                Player2ConsecutiveHits++;
            }
        }
        else
        {
            // Errou o tiro: Reseta o Streak do jogador atual
            if (playerId == Player1Id) Player1ConsecutiveHits = 0;
            else Player2ConsecutiveHits = 0;
        }

        // 3. Verificação de Vitória ou Troca de Turno
        if (targetBoard.AllShipsSunk())
        {
            FinishGame(playerId);
        }
        else if (!isHit)
        {
            // Se errar, passa a vez.
            SwitchTurn();
        }
        else
        {
            HasMovedThisTurn = false;
        }

        LastMoveAt = DateTime.UtcNow;
        return isHit;
    }

    // Mover Navio (Apenas Modo Dinâmico)
    public void ExecuteShipMovement(Guid playerId, Guid shipId, MoveDirection direction)
    {
        if (Mode != GameMode.Dynamic)
            throw new InvalidOperationException("Movimentação de navios só é permitida no modo Dinâmico.");

        ValidateTurn(playerId);

        if (HasMovedThisTurn)
            throw new InvalidOperationException("Você já realizou um movimento neste turno. Agora deve atirar.");

        var myBoard = playerId == Player1Id ? Player1Board : Player2Board;

        // Tenta mover. Se falhar (colisão/navio atingido), o Board lança exceção e o turno NÃO muda.
        myBoard.MoveShip(shipId, direction);

        HasMovedThisTurn = true;
        //Atualizamos o tempo para evitar Timeout enquanto o jogador pensa no tiro
        LastMoveAt = DateTime.UtcNow;
    }

    private void ValidateTurn(Guid playerId)
    {
        if (Status != MatchStatus.InProgress) throw new InvalidOperationException("A partida não está em andamento.");
        if (IsFinishedOrTimeout()) throw new InvalidOperationException("Partida finalizada ou tempo esgotado.");
        
        // Verifica se é o turno do jogador (Permite IA jogar se playerId for Empty)
        if (playerId != Guid.Empty && playerId != CurrentTurnPlayerId) 
            throw new InvalidOperationException("Não é o seu turno.");

        // Validação de tempo corrigida (30s de tolerância + 1s de margem)
        if (DateTime.UtcNow.Subtract(LastMoveAt).TotalSeconds > 31) 
        {
            SwitchTurn(); // Penalidade por tempo: perde a vez
            // Opcional: throw new TimeoutException("Tempo esgotado.");
        }
    }

    private bool IsFinishedOrTimeout()
    {
        return Status == MatchStatus.Finished;
    }

    private void SwitchTurn()
    {
        CurrentTurnPlayerId = CurrentTurnPlayerId == Player1Id
            ? Player2Id ?? Guid.Empty // Passa para P2 ou IA
            : Player1Id;              // Passa para P1
        
        HasMovedThisTurn = false;
    }

    private void FinishGame(Guid winnerId)
    {
        Status = MatchStatus.Finished;
        WinnerId = winnerId;
        FinishedAt = DateTime.UtcNow;
        CurrentTurnPlayerId = Guid.Empty; // Ninguém mais joga
    }
}
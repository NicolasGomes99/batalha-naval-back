using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BatalhaNaval.Domain.Entities;
using Newtonsoft.Json;

namespace BatalhaNaval.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.Player1Id).HasColumnName("player1_id");
        builder.Property(m => m.Player2Id).HasColumnName("player2_id");
        builder.Property(m => m.WinnerId).HasColumnName("winner_id");
        builder.Property(m => m.StartedAt).HasColumnName("started_at");
        builder.Property(m => m.LastMoveAt).HasColumnName("last_move_at"); // Precisamos adicionar essa coluna no SQL ou via Migration
        builder.Property(m => m.CurrentTurnPlayerId).HasColumnName("current_turn_player_id");
        
        // Conversão de Enums para String (Mais legível no banco)
        builder.Property(m => m.Mode)
            .HasConversion<string>()
            .HasColumnName("game_mode");

        builder.Property(m => m.AiDifficulty)
            .HasConversion<string>()
            .HasColumnName("ai_difficulty");
            
        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasColumnName("status"); // Nova coluna necessária

        // --- MAPEAMENTO COMPLEXO JSONB ---
        // Serializamos o objeto Board inteiro para JSON ao salvar
        // E deserializamos de volta para objeto ao ler.
        
        builder.Property(m => m.Player1Board)
            .HasColumnName("player1_board_json") // Coluna nova para armazenar estado
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Board>(v) ?? new Board()
            );

        builder.Property(m => m.Player2Board)
            .HasColumnName("player2_board_json") // Coluna nova para armazenar estado
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Board>(v) ?? new Board()
            );

        // Ignoramos a propriedade computada IsFinished para não criar coluna
        builder.Ignore(m => m.IsFinished);
       // builder.Ignore(m => m.CurrentTurnPlayerId); Opcional: Persistir ou calcular? Vamos persistir para segurança.
    }
}
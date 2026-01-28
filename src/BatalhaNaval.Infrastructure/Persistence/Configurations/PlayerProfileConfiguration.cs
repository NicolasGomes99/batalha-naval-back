using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BatalhaNaval.Domain.Entities;
using Newtonsoft.Json;

namespace BatalhaNaval.Infrastructure.Persistence.Configurations;

public class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        builder.ToTable("player_profiles");

        builder.HasKey(p => p.UserId);

        builder.Property(p => p.UserId).HasColumnName("user_id");
        builder.Property(p => p.RankPoints).HasColumnName("rank_points");
        builder.Property(p => p.Wins).HasColumnName("wins");
        builder.Property(p => p.Losses).HasColumnName("losses");
        builder.Property(p => p.CurrentStreak).HasColumnName("current_streak");
        builder.Property(p => p.MaxStreak).HasColumnName("max_streak");

        // Medalhas como lista de strings (JSONB array)
        // Isso evita precisar fazer join com tabela de medalhas para leitura simples
        builder.Property(p => p.EarnedMedalCodes)
            .HasColumnName("medals_json") 
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>()
            );
            
        // Propriedade computada ignorada
        builder.Ignore(p => p.WinRate);
    }
}
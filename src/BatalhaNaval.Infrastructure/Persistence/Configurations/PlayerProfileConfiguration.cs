using BatalhaNaval.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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


        // 1. Conversor Explícito: List<string> <-> String (JSON)
        var medalConverter = new ValueConverter<List<string>, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>()
        );

        // 2. Comparador de Valor: Ensina o EF a detectar mudanças na lista
        var medalComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(p => p.EarnedMedalCodes)
            .HasColumnName("medals_json")
            .HasColumnType("jsonb")
            .HasConversion(medalConverter)
            .Metadata.SetValueComparer(medalComparer);

        builder.Ignore(p => p.WinRate);
    }
}
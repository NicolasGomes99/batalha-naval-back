using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BatalhaNaval.Domain.Entities;

[Table("player_profiles")]
public class PlayerProfile
{
    [Key]
    [ForeignKey(nameof(User))]
    [Column("user_id")]
    public Guid UserId { get; set; }

    public virtual User? User { get; set; }

    [Column("rank_points")]
    public int RankPoints { get; set; }

    [Column("wins")]
    public int Wins { get; set; }

    [Column("losses")]
    public int Losses { get; set; }

    [Column("current_streak")]
    public int CurrentStreak { get; set; }

    [Column("max_streak")]
    public int MaxStreak { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped] 
    public double WinRate => (Wins + Losses) == 0 ? 0 : (double)Wins / (Wins + Losses);

    [NotMapped]
    public List<string> EarnedMedalCodes { get; set; } = new();

    public void AddWin(int points)
    {
        Wins++;
        CurrentStreak++;
        if (CurrentStreak > MaxStreak) MaxStreak = CurrentStreak;
        RankPoints += points;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddLoss()
    {
        Losses++;
        CurrentStreak = 0;
        // RankPoints -= points? (Regra de perda de pontos opcional)
        UpdatedAt = DateTime.UtcNow;
    }
}
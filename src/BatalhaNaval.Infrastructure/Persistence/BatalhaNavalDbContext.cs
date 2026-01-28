using Microsoft.EntityFrameworkCore;
using BatalhaNaval.Domain.Entities;
using BatalhaNaval.Infrastructure.Persistence.Configurations;

namespace BatalhaNaval.Infrastructure.Persistence;

public class BatalhaNavalDbContext : DbContext
{
    public BatalhaNavalDbContext(DbContextOptions<BatalhaNavalDbContext> options) : base(options)
    {
    }

    public DbSet<Match> Matches { get; set; }
    public DbSet<PlayerProfile> PlayerProfiles { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica as configurações separadas (Mapeamento)
        modelBuilder.ApplyConfiguration(new MatchConfiguration());
        modelBuilder.ApplyConfiguration(new PlayerProfileConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}
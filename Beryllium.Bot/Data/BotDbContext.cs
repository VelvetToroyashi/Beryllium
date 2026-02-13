using Microsoft.EntityFrameworkCore;
using Beryllium.Bot.Models;
using Beryllium.Bot.Models.Entities;

namespace Beryllium.Bot.Data;

public class BotDbContext
(
    DbContextOptions<BotDbContext> options
) : DbContext(options)
{
    public DbSet<GuildSettings> Guilds => Set<GuildSettings>();
    public DbSet<InfractionEntity> Infractions => Set<InfractionEntity>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GuildSettings>(entity =>
        {
            entity.HasKey(e => e.GuildId);
            entity.Property(e => e.GuildId).ValueGeneratedNever();
        });
    }
}

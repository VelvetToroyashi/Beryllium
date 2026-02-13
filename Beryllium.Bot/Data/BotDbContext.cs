using Microsoft.EntityFrameworkCore;
using Beryllium.Bot.Models;

namespace Beryllium.Bot.Data;

/// <summary>
/// Database context for the Beryllium bot.
/// </summary>
public class BotDbContext : DbContext
{
    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
    {
    }

    public DbSet<GuildSettings> GuildSettings => Set<GuildSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GuildSettings>(entity =>
        {
            entity.HasKey(e => e.GuildId);
            entity.Property(e => e.Prefix).HasMaxLength(10).HasDefaultValue("!");
        });
    }
}

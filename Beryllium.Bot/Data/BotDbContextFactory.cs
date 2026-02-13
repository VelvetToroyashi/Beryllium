using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Beryllium.Bot.Data;

/// <summary>
/// Factory for creating DbContext instances at design time for migrations.
/// </summary>
public class BotDbContextFactory : IDesignTimeDbContextFactory<BotDbContext>
{
    public BotDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BotDbContext>();
        optionsBuilder.UseSqlite("Data Source=beryllium.db");

        return new BotDbContext(optionsBuilder.Options);
    }
}

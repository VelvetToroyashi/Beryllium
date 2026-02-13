using Beryllium.Bot.Data;
using Beryllium.Bot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Hosting.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables(prefix: "BERYLLIUM_");
    })
    .ConfigureServices((context, services) =>
    {
        // Configure bot settings
        var botConfig = context.Configuration.GetSection("Bot").Get<BotConfiguration>() 
            ?? throw new InvalidOperationException("Bot configuration is missing.");

        services.AddSingleton(botConfig);

        // Configure database
        services.AddDbContext<BotDbContext>(options =>
            options.UseSqlite(botConfig.DatabaseConnection));

        // Configure Remora.Discord
        services.AddDiscordService(_ => botConfig.Token);

        // Add command support
        services.AddCommandTree()
            .WithCommandGroup<Beryllium.Bot.Commands.PingCommands>();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BotDbContext>();
    await db.Database.EnsureCreatedAsync();
}

await host.RunAsync();

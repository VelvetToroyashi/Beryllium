namespace Beryllium.Bot;

/// <summary>
/// Configuration settings for the Discord bot.
/// </summary>
public class BotConfiguration
{
    /// <summary>
    /// The Discord bot token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The database connection string.
    /// </summary>
    public string DatabaseConnection { get; set; } = "Data Source=beryllium.db";
}

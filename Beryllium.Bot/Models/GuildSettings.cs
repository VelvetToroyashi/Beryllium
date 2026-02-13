namespace Beryllium.Bot.Models;

/// <summary>
/// Represents guild-specific settings stored in the database.
/// </summary>
public class GuildSettings
{
    /// <summary>
    /// The guild ID (Primary Key).
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// The command prefix for this guild.
    /// </summary>
    public string Prefix { get; set; } = "!";

    /// <summary>
    /// Whether moderation features are enabled.
    /// </summary>
    public bool ModerationEnabled { get; set; } = true;
}

using Remora.Rest.Core;

namespace Beryllium.Bot.Models;

/// <summary>
/// Represents guild-specific settings stored in the database.
/// </summary>
public class GuildSettings
{
    /// <summary>
    /// The guild ID (Primary Key).
    /// </summary>
    public Snowflake GuildId { get; set; }
    
    
}

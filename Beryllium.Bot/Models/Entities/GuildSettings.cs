using Remora.Rest.Core;

namespace Beryllium.Bot.Models.Entities;

/// <summary>
/// Represents guild-specific settings stored in the database.
/// </summary>
public class GuildSettings
{
    /// <summary>
    /// The guild ID.
    /// </summary>
    public Snowflake GuildId { get; set; }
    
    /// <summary>
    /// The ID of the channel to log infractions to. 
    /// </summary>
    public Snowflake? LogChannelId { get; set; }
    
    
}

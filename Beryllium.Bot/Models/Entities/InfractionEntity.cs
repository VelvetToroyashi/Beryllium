using Remora.Rest.Core;
using Remora.Results;

namespace Beryllium.Bot.Models.Entities;

using System;

public enum InfractionType
{
    Warning,
    Mute,
    Kick,
    Ban,
    Unban,
    Unmute,
    Pardon
}

public class InfractionEntity
{
    public int Id { get; set; }
    
    // TODO: Figure out how best to actually handle these properties?
    // considering we're going the 'factory method' route, we probably want to private the setters here.
    // iirc, EFCore will just thug it out. 
    public required Snowflake GuildId { get; set; }
    public required Snowflake UserId { get; set; }
    public required Snowflake ModeratorId { get; set; }
    public required string Reason { get; set; }
    public required InfractionType Type { get; set; } 
    
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { set; get; }
    
    // Automated in this context refers to say, Discord AutoMod triggers -> Create an associated warning automatically
    // OR external bot bans user -> track that as well; UserID + Reason tells us this much, but this also allows for,
    // say, choosing to ignore automated bans and/or logging them, since another bot would likely already have
    // logging for such. We'll put actual documentation to this once the bot is more fleshed out.
    public bool IsAutomated { get; set; }
    
    // Not shown in /case log, but can still be queried directly.
    public bool IsHidden { get; set; }
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;
    // This is the one we care about.
    public bool IsActive => !IsHidden && !IsPardoned && !IsExpired;
    
    public bool IsPardoned { get; set; }
    public int? PardonId { get; set; }
    public Snowflake? PardonedBy { get; set; }
    
    private static Result<InfractionEntity> CreateInternal
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake moderatorId,
        InfractionType type,
        string? reason,
        bool isAutomated = false,
        TimeSpan? duration = null
    )
    {
        if (duration < TimeSpan.Zero)
        {
            return new InvalidOperationError("Duration must be greater than zero.");
        }

        var infraction = new InfractionEntity
        {
            GuildId = guildId,
            UserId = userId,
            ModeratorId = moderatorId,
            Type = type,
            Reason = reason ?? "No reason provided."
        };

        if (duration is not null)
        {
            infraction.ExpiresAt = DateTime.UtcNow + duration;
        }

        return infraction;
    }
    
    public static Result<InfractionEntity> CreateWarning
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake moderatorId,
        string? reason = null,
        bool isAutomated = false
    ) => CreateInternal(guildId, userId, moderatorId, InfractionType.Warning, reason, isAutomated);

    public static Result<InfractionEntity> CreateKick
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake moderatorId,
        string? reason = null,
        bool isAutomated = false
    ) => CreateInternal(guildId, userId, moderatorId, InfractionType.Kick, reason, isAutomated);

    public static Result<InfractionEntity> CreateBan
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake moderatorId,
        TimeSpan? duration = null,
        string? reason = null,
        bool isAutomated = false
    ) => CreateInternal(guildId, userId, moderatorId, InfractionType.Ban, reason, isAutomated, duration);

    public static Result<InfractionEntity> CreateMute
    (
        Snowflake guildId,
        Snowflake moderatorId,
        Snowflake userId,
        TimeSpan? duration = null,
        string? reason = null,
        bool isAutomated = false
    ) => CreateInternal(guildId, userId, moderatorId, InfractionType.Mute, reason, isAutomated, duration);

    public static Result<InfractionEntity> CreateUnban
    (
        Snowflake guildId,
        Snowflake moderatorId,
        Snowflake userId,
        string? reason = null,
        bool isAutomated = false
    ) => CreateInternal(guildId, userId, moderatorId, InfractionType.Unban, reason);
    
    public static Result<InfractionEntity> CreateUnmute
    (
        Snowflake guildId,
        Snowflake moderatorId,
        Snowflake userId,
        string? reason = null,
        bool isAutomated = false
    ) => CreateInternal(guildId, userId, moderatorId, InfractionType.Unmute, reason, isAutomated);

    public static Result<InfractionEntity> CreatePardon
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake moderatorId,
        int infractionId,
        string? reason = null,
        bool isAutomated = false
    )
    {
        var infraction = new InfractionEntity
        {
            GuildId = guildId,
            UserId = userId,
            ModeratorId = moderatorId,
            Type = InfractionType.Pardon,
            Reason = reason ?? "No reason provided.",
            PardonId = infractionId,
            IsAutomated = isAutomated
        };

        return infraction;
    }
    
    public Result UpdateExpiration
    (
        DateTimeOffset? newExpiration
    )
    {
        if (newExpiration < ExpiresAt)
        {
            return new InvalidOperationError("New expiration must be in the future. Set to `null` to remove expiration");
        }
        
        ExpiresAt = newExpiration;

        return Result.Success;
    }
}

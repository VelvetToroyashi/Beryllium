using Beryllium.Bot.Data;
using Beryllium.Bot.Models.DTO;
using Mediator;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Extensions.Formatting;
using Remora.Rest.Core;

namespace Beryllium.Bot.Features.Logging.Notifications;

public class InfractionNotificationHandler(BotDbContext dbContext, IDiscordRestChannelAPI channels) 
    : INotificationHandler<InfractionCreatedNotification>, INotificationHandler<InfractionUpdatedNotification>
{
    // In the long term, I'd like to structure this in a way that lets us queue up logs when dealing with backpressure/ratelimits, but, doubt we'll ever be that big.
    public async ValueTask Handle
    (
        InfractionCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        var guildConfig = await dbContext.Guilds.FindAsync([notification.Infraction.GuildId], cancellationToken: cancellationToken);

        // Shouldn't be, but, *y'know.*
        if (guildConfig is null)
        {
            throw new InvalidOperationException($"Received an infraction creation, but could not find the associated guild to log to ({notification.Infraction.GuildId})!");
        }

        if (guildConfig.LogChannelId is null)
        {
            return;
        }
        
        var getChannelResult = await channels.GetChannelAsync(guildConfig.LogChannelId.Value, ct: cancellationToken);

        if (!getChannelResult.IsSuccess)
            return;
        
        var channel = getChannelResult.Entity;
        await channels.CreateMessageAsync(channel.ID, embeds: GetEmbedForInfraction(notification.Infraction), ct: cancellationToken);
    }

    private Optional<IReadOnlyList<IEmbed>> GetEmbedForInfraction
    (
        InfractionDTO notificationInfraction
    )
    {
        List<IEmbedField> embedFields =
        [
            new EmbedField("Moderator", $"<@{notificationInfraction.ModeratorId}>", true),
            new EmbedField("Target", $"<@{notificationInfraction.UserId}>", true),
            new EmbedField("Type", notificationInfraction.Type.ToString(), true),
            new EmbedField("Automatic?", notificationInfraction.IsAutomated.ToString(), true)
        ];

        if (notificationInfraction.ExpiresAt is not null)
        {
            embedFields.Add(new EmbedField("Expires", Markdown.Timestamp(notificationInfraction.ExpiresAt!.Value, TimestampStyle.RelativeTime), true));
        }
        
        
        var embed = new Embed
        {
            Title = $"Case #{notificationInfraction.CaseId}",
            Fields = new (embedFields),
            Description = notificationInfraction.Reason
        };

        return new([embed]);
    }

    public async ValueTask Handle
    (
        InfractionUpdatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
    
    
}
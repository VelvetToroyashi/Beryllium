using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Beryllium.Bot.Models.DTO;
using Beryllium.Bot.Models.Entities;
using FluentValidation;
using Mediator;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Extensions.Formatting;
using Remora.Rest.Core;
using Remora.Results;

namespace Beryllium.Bot.Features.Moderation.Commands;

public static class NotifyUserOfInfraction
{
    // TODO: Inquire whether we should keep it parameterized like this (cleaner for reuse) or if passing our DTO is acceptable.
    public record Command(InfractionDTO Infraction) : ICommand<Result>;

    internal class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleSet
            (
                "ID Validation",
                () => {
                    RuleFor(x => x.Infraction.GuildId).NotEmpty(); // Ensure nonzero AND a valid snowflake (default = discord epoch)
                    RuleFor(x => x.Infraction.ModeratorId).NotEmpty();
                    RuleFor(x => x.Infraction.UserId).NotEmpty();
                    RuleFor(x => x.Infraction.ModeratorId)
                        .NotEqual(x => x.Infraction.UserId)
                        .WithMessage("Moderators cannot invoke commands on themselves!");
                }
            );
            
            RuleFor(x => x.Infraction.Reason).NotEmpty();
            RuleFor(x => x.Infraction.ExpiresAt).GreaterThan(DateTimeOffset.UtcNow).Unless(x => x.Infraction.ExpiresAt is null);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Handler(IDiscordRestUserAPI userApi, IDiscordRestGuildAPI guilds, IDiscordRestChannelAPI channelsApi) : ICommandHandler<Command, Result>
    {
        
        public async ValueTask<Result> Handle
        (
            Command command,
            CancellationToken cancellationToken
        )
        {
            var getGuildResult = await guilds.GetGuildAsync(command.Infraction.GuildId, withCounts:false, CancellationToken.None);
            
            if (!getGuildResult.IsSuccess)
                // Technically exceptional; we expect the guild to either always be available, or cached.
                // Discord, however is known for many things; being a stable platform is not one.
                return Result.FromError(getGuildResult.Error); 
            
            var createChannelResult = await userApi.CreateDMAsync(command.Infraction.UserId, cancellationToken);
            
            if (!createChannelResult.IsSuccess)
                return Result.FromError(createChannelResult.Error);
            
            var channel = createChannelResult.Entity;
            var components = GetComponentsForInfraction(command, getGuildResult.Entity);
            var notificationResult = await channelsApi.CreateMessageAsync(channel.ID, components: new(components), ct: cancellationToken);

            return (Result)notificationResult;
        }

        private static IReadOnlyList<IMessageComponent> GetComponentsForInfraction
        (
            Command command,
            IGuild guild
        )
        {
            var infraction = command.Infraction;
            
            var receievedInfractionString = infraction.Type switch
            {
                InfractionType.Kick => $"You have been **kicked** from **{guild.Name}**!",
                InfractionType.Ban => $"You have been **banned** from **{guild.Name}**!",
                InfractionType.Mute => $"You have been **muted** in **{guild.Name}**!",
                InfractionType.Warning => $"You have been **warned** in **{guild.Name}**!",
                InfractionType.Pardon => $"You have been **pardoned** in **{guild.Name}**!",
                InfractionType.Unban => $"You have been **unbanned** from **{guild.Name}**!",
                InfractionType.Unmute => $"You have been **unmuted** in **{guild.Name}**!",
                _ => throw new UnreachableException()
            };

            var infractionExpirationHelpText = infraction.Type switch
            {
                InfractionType.Ban when infraction.ExpiresAt is null => "This ban is permanent. You may not rejoin the server unless unbanned.",
                InfractionType.Ban =>
                    $"You may rejoin the server {Markdown.Timestamp(infraction.ExpiresAt.GetValueOrDefault(), TimestampStyle.LongDateTime)} ({Markdown.Timestamp(infraction.ExpiresAt.GetValueOrDefault(), TimestampStyle.RelativeTime)})",
                InfractionType.Warning => "Future warnings may entail further consequences.",
                _ => "N/A"
            };

            IMessageComponent[] components = [
                new TextDisplayComponent(receievedInfractionString), 
                new SeparatorComponent(IsDivider: true, Spacing: SeparatorSpacingSize.Small),
                new TextDisplayComponent(
                    $"""
                         **Moderator**: <@{infraction.ModeratorId}>
                         **Reason**: {infraction.Reason}
                         **Additional information**: {infractionExpirationHelpText}
                     """)
            ];

            var accentColor = infraction.Type switch
            {
                InfractionType.Warning => Color.Goldenrod,
                InfractionType.Kick or InfractionType.Ban or InfractionType.Mute => Color.DarkRed,
                InfractionType.Pardon or InfractionType.Unban or InfractionType.Unmute => Color.DarkGreen,
                _ => throw new UnreachableException()
            };

            var sectionComponent = new ContainerComponent(components, AccentColour: accentColor);
            return [sectionComponent];
        }
    }
}
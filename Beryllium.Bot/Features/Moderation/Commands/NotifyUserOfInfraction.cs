using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
    public record Command
    (
        Snowflake GuildId,
        Snowflake ModeratorId,
        Snowflake UserId,
        InfractionType Type,
        string Reason,
        DateTimeOffset? ExpiresAt
    ) : ICommand<Result>;

    internal class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleSet
            (
                "ID Validation",
                () => {
                    RuleFor(x => x.GuildId).NotEmpty(); // Ensure nonzero AND a valid snowflake (default = discord epoch)
                    RuleFor(x => x.ModeratorId).NotEmpty();
                    RuleFor(x => x.UserId).NotEmpty();
                    RuleFor(x => x.ModeratorId)
                        .NotEqual(x => x.UserId)
                        .WithMessage("Moderators cannot invoke commands on themselves!");
                }
            );
            
            RuleFor(x => x.Reason).NotEmpty();
            RuleFor(x => x.ExpiresAt).GreaterThan(DateTimeOffset.UtcNow).Unless(x => x.ExpiresAt is null);
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
            var getGuildResult = await guilds.GetGuildAsync(command.GuildId, withCounts:false, CancellationToken.None);
            
            if (!getGuildResult.IsSuccess)
                // Technically exceptional; we expect the guild to either always be available, or cached.
                // Discord, however is known for many things; being a stable platform is not one.
                return Result.FromError(getGuildResult.Error); 
            
            var createChannelResult = await userApi.CreateDMAsync(command.UserId, cancellationToken);
            
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
            var receievedInfractionString = command.Type switch
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

            var infractionExpirationHelpText = command.Type switch
            {
                InfractionType.Ban when command.ExpiresAt is null => "This ban is permanent. You may not rejoin the server unless unbanned.",
                InfractionType.Ban =>
                    $"You may rejoin the server {Markdown.Timestamp(command.ExpiresAt.GetValueOrDefault(), TimestampStyle.LongDateTime)} ({Markdown.Timestamp(command.ExpiresAt.GetValueOrDefault(), TimestampStyle.RelativeTime)})",
                InfractionType.Warning => "Future warnings may entail further consequences.",
                _ => "N/A"
            };

            IMessageComponent[] components = [
                new TextDisplayComponent(receievedInfractionString), 
                new SeparatorComponent(IsDivider: true, Spacing: SeparatorSpacingSize.Small),
                new TextDisplayComponent(
                    $"""
                         **Moderator**: <@{command.ModeratorId}>
                         **Reason**: {command.Reason}
                         **Additional information**: {infractionExpirationHelpText}
                     """)
            ];

            var accentColor = command.Type switch
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
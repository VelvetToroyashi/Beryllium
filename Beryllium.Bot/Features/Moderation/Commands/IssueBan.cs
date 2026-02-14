using System.ComponentModel;
using Beryllium.Bot.Data;
using Beryllium.Bot.Features.Logging.Notifications;
using Beryllium.Bot.Models.Entities;
using FluentValidation;
using Mediator;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Extensions.Formatting;
using Remora.Rest.Core;

namespace Beryllium.Bot.Features.Moderation.Commands;

public static class IssueBan
{
    public record Command
    (
        Snowflake GuildId,
        Snowflake ModeratorId,
        Snowflake UserId,
        TimeSpan? Duration,
        TimeSpan? DeleteMessageTime,
        bool IsAutomated,
        string Reason
    ) : IInfractionCommand;

    internal class Validator : AbstractValidator<Command>
    {
        public Validator
        (
            IValidator<IInfractionCommand> baseValidation
        )
        {
            Include(baseValidation);
            RuleFor(x => x.Duration).GreaterThan(TimeSpan.Zero).Unless(x => x.Duration is null);
            RuleFor(x => x.DeleteMessageTime).InclusiveBetween(TimeSpan.Zero, TimeSpan.FromDays(14));
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Handler
    (
        IMediator mediator,
        IDiscordRestGuildAPI guildApi,
        BotDbContext dbContext
    ) : ICommandHandler<Command, Result>
    {
        public async ValueTask<Result> Handle
        (
            Command command,
            CancellationToken cancellationToken
        )
        {
            command.Deconstruct(out var guildId, out var targetId, out var moderatorId, out var duration, out var deleteMessageTime, out var isAutomated, out string reason);
            
            if (!string.IsNullOrEmpty(reason))
            {
                reason = Markdown.Sanitize(reason);
            }

            var infractionResult = InfractionEntity.CreateBan(guildId, targetId, moderatorId, duration, reason, isAutomated);

            if (!infractionResult.IsSuccess)
                return Result.FromError(infractionResult.Error);
            
            var infraction = infractionResult.Entity;

            dbContext.Infractions.Add(infraction);
            await dbContext.SaveChangesAsync(cancellationToken);

            var infractionDTO = infraction.ToDTO();
            
            await mediator.Send(new NotifyUserOfInfraction.Command(infractionDTO), cancellationToken);
            await mediator.Send(new InfractionCreatedNotification(infractionDTO), cancellationToken);

            Optional<int> deleteMessageSeconds = default;
            if (deleteMessageTime.HasValue)
            {
                deleteMessageSeconds = (int)deleteMessageTime.Value.TotalSeconds;
            }

            var banResult = await guildApi.CreateGuildBanAsync(command.GuildId, targetId, deleteMessageSeconds, reason[..100], cancellationToken);
            
            return banResult;
        }
    }
}
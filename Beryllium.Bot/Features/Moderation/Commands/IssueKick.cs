using System.ComponentModel;
using Beryllium.Bot.Data;
using Beryllium.Bot.Features.Logging.Notifications;
using Beryllium.Bot.Models.DTO;
using Beryllium.Bot.Models.Entities;
using FluentValidation;
using Mediator;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Extensions.Formatting;
using Remora.Rest.Core;
using Remora.Results;

namespace Beryllium.Bot.Features.Moderation.Commands;

public static class IssueKick
{
    public record Command(Snowflake GuildId, Snowflake ModeratorId, Snowflake UserId, bool IsAutomated, string Reason) : ICommand<Result>;

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
                }
            );
            
            RuleFor(x => x.Reason).NotEmpty();
            RuleFor(x => x.ModeratorId)
                .NotEqual(x => x.UserId)
                .WithMessage("Moderators cannot invoke commands on themselves!");
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Handler 
    (
        IMediator mediator,
        IDiscordRestGuildAPI guildApi,
        BotDbContext dbContextFactory
    ) : ICommandHandler<Command, Result>
    {
        public async ValueTask<Result> Handle
        (
            Command command,
            CancellationToken cancellationToken
        )
        {
            // TODO: Set up pipelining to wrap these, and validate permissions, 
            // instead of littering every handler with duplicate code.
            // Need to check if we can grab attributes, otherwise it'll be a helper method in a service.
            var commandReason = command.Reason;
            if (!string.IsNullOrEmpty(commandReason))
            {
                commandReason = Markdown.Sanitize(commandReason);
            }
            
            var infractionResult = InfractionEntity.CreateKick(command.GuildId, command.UserId, command.ModeratorId, commandReason, command.IsAutomated);
            
            if (!infractionResult.IsSuccess)
                return Result.FromError(infractionResult.Error);

            var infraction = infractionResult.Entity;

            dbContextFactory.Infractions.Add(infraction);
            await dbContextFactory.SaveChangesAsync(cancellationToken);
            
            await mediator.Send(new NotifyUserOfInfraction.Command(command.GuildId, command.ModeratorId, command.UserId, InfractionType.Kick, commandReason,  ExpiresAt: null), cancellationToken);
            await mediator.Send(new InfractionCreatedNotification(infraction.ToDTO()), cancellationToken);
            
            var kickResult = await guildApi.RemoveGuildMemberAsync(command.GuildId, command.UserId, commandReason[..100], cancellationToken);


            return kickResult;
        }
    }
}
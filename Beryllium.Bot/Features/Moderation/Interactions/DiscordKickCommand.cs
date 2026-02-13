using System.ComponentModel;
using Beryllium.Bot.Features.Moderation.Commands;
using Mediator;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Results;

namespace Beryllium.Bot.Features.Moderation.Interactions;

public class DiscordKickCommand(IInteractionCommandContext context, IMediator mediator, IDiscordRestInteractionAPI interactions) : CommandGroup
{
    [Ephemeral]
    [Command("kick")]
    [Description("Kicks a user.")]
    public async Task<IResult> KickAsync
    (
        [Description("The user to kick")]
        IUser target,
        [Description("The reason for the kick")]
        string reason
    )
    {
        _ = context.TryGetUserID(out var moderatorId);
        _ = context.TryGetGuildID(out var guildId);
        var applicationId = context.Interaction.ApplicationID;
        var interactionToken = context.Interaction.Token;
        var result = await mediator.Send(new IssueKick.Command(guildId, moderatorId, target.ID, IsAutomated: false, reason));

        var message = result.IsSuccess ? "User kicked successfully!" : $"Failed to kick user!\n{result.Error.Message}";
        return (Result)await interactions.CreateFollowupMessageAsync(applicationId, interactionToken, message);
    }
    
}
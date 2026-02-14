using System.ComponentModel;
using Beryllium.Bot.Features.Moderation.Commands;
using Mediator;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Results;

namespace Beryllium.Bot.Features.Moderation.Interactions;

public class DiscordBanCommand(IInteractionCommandContext context, IMediator mediator, IDiscordRestInteractionAPI interactions)  : CommandGroup
{
    [Command("ban")]
    [Description("Bans a user.")]
    public async Task<IResult> BanAsync
    (
        IUser user,
        string reason,
        BanDuration? duration,
        BanDeleteMessages deleteMessages = BanDeleteMessages.None
    )
    {
        TimeSpan? durationTs = duration switch
        {
            BanDuration.OneDay => TimeSpan.FromDays(1),
            BanDuration.ThreeDays => TimeSpan.FromDays(3),
            BanDuration.OneWeek => TimeSpan.FromDays(7),
            BanDuration.TwoWeeks => TimeSpan.FromDays(14),
            BanDuration.OneMonth => TimeSpan.FromDays(30),
            _ => null
        };
        
        TimeSpan? deleteMessagesTs = deleteMessages switch
        {
            BanDeleteMessages.OneHour => TimeSpan.FromHours(1),
            BanDeleteMessages.OneDay  => TimeSpan.FromDays(1),
            BanDeleteMessages.ThreeDays => TimeSpan.FromDays(3),
            BanDeleteMessages.OneWeek => TimeSpan.FromDays(7),
            BanDeleteMessages.TwoWeeks => TimeSpan.FromDays(14),
            _ => null
        };
        
        _ = context.TryGetUserID(out var moderatorId);
        _ = context.TryGetGuildID(out var guildId);
        var applicationId = context.Interaction.ApplicationID;
        var interactionToken = context.Interaction.Token;

        var result = await mediator.Send(new IssueBan.Command(guildId, moderatorId, user.ID, durationTs, deleteMessagesTs, IsAutomated: false, reason));
        
        var message = result.IsSuccess ? "User banned successfully!" : $"Failed to ban user!\n{result.Error.Message}";
        return (Result)await interactions.CreateFollowupMessageAsync(applicationId, interactionToken, message);
    }
}

public enum BanDuration
{
    OneDay,
    ThreeDays,
    OneWeek,
    TwoWeeks,
    OneMonth
}

public enum BanDeleteMessages
{
    None,
    OneHour,
    OneDay,
    ThreeDays,
    OneWeek,
    TwoWeeks
}



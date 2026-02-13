using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Beryllium.Bot.Commands;

/// <summary>
/// Basic ping command group for testing.
/// </summary>
public class PingCommands : CommandGroup
{
    private readonly FeedbackService _feedbackService;

    public PingCommands(FeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [Command("ping")]
    [Description("Responds with pong!")]
    public async Task<IResult> PingAsync()
    {
        return await _feedbackService.SendContextualSuccessAsync("Pong! 🏓");
    }
}

using JetBrains.Annotations;
using Mediator;
using Remora.Rest.Core;

namespace Beryllium.Bot.Features.Moderation;

[PublicAPI]
public interface IInfractionCommand : ICommand<Result>
{
    Snowflake GuildId { get; }
    Snowflake ModeratorId { get; }
    Snowflake UserId { get; }
    string Reason { get; }
    bool IsAutomated { get; }
}
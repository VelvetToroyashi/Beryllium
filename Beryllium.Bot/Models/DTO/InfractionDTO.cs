using Beryllium.Bot.Models.Entities;
using Remora.Rest.Core;

namespace Beryllium.Bot.Models.DTO;

public record InfractionDTO
(
    int CaseId,
    InfractionType Type,
    Snowflake UserId,
    Snowflake GuildId,
    Snowflake ModeratorId,
    DateTimeOffset? ExpiresAt,
    string Reason,
    bool IsActive,
    InfractionStatus Status
);

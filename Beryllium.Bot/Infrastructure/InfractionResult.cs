using Beryllium.Bot.Models.Entities;
using Remora.Rest.Core;

namespace Beryllium.Bot.Infrastructure;

public record InfractionResult
(  
    int CaseId,
    InfractionType Type,
    Snowflake UserId,
    Snowflake ModeratorId,
    DateTimeOffset? ExpiresAt,
    string Reason,
    bool UserNotified
    
);
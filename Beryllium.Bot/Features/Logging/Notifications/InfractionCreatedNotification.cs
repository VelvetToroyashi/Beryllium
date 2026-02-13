using Beryllium.Bot.Models.DTO;
using Mediator;

namespace Beryllium.Bot.Features.Logging.Notifications;

/// <summary>
/// Represents that an infraction was newly created.
/// </summary>
/// <param name="Infraction">The infraction.</param>
public record InfractionCreatedNotification(InfractionDTO Infraction) : INotification;
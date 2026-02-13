using Beryllium.Bot.Models.DTO;
using Mediator;

namespace Beryllium.Bot.Features.Logging.Notifications;

public record InfractionUpdatedNotification (InfractionDTO Infraction) : INotification;
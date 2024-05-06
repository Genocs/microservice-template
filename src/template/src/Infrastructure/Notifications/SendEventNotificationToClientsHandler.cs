using Genocs.Microservice.Template.Application.Common.Events;
using Genocs.Microservice.Template.Application.Common.Interfaces;
using MediatR;

namespace Genocs.Microservice.Template.Infrastructure.Notifications;

/// <summary>
/// Sends all events that are also an INotificationMessage to all clients.
/// Note: for this to work, the Event/NotificationMessage class needs to be in the
/// shared project (i.e. have the same FullName - so with namespace - on both sides).
/// </summary>
/// <typeparam name="TNotification">The notification type.</typeparam>
public class SendEventNotificationToClientsHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    private readonly INotificationSender _notifications;

    public SendEventNotificationToClientsHandler(INotificationSender notifications) =>
        _notifications = notifications;

    public Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        var notificationType = typeof(TNotification);
        if (notificationType.IsGenericType
            && notificationType.GetGenericTypeDefinition() == typeof(EventNotification<>)
            && notificationType.GetGenericArguments()[0] is { } eventType
            && eventType.IsAssignableTo(typeof(INotificationMessage)))
        {
            INotificationMessage notificationMessage = ((dynamic)notification).Event;
            return _notifications.SendToAllAsync(notificationMessage, cancellationToken);
        }

        return Task.CompletedTask;
    }
}
using Genocs.Microservice.Template.Domain.Common.Events;
using Genocs.Microservice.Template.Domain.Identity;
using Genocs.Microservice.Template.Shared.Events;

namespace Genocs.Microservice.Template.Application.Dashboard;

public class SendStatsChangedNotificationHandler :
    IEventNotificationHandler<EntityCreatedEvent<Brand>>,
    IEventNotificationHandler<EntityDeletedEvent<Brand>>,
    IEventNotificationHandler<EntityCreatedEvent<Product>>,
    IEventNotificationHandler<EntityDeletedEvent<Product>>,
    IEventNotificationHandler<ApplicationRoleCreatedEvent>,
    IEventNotificationHandler<ApplicationRoleDeletedEvent>,
    IEventNotificationHandler<ApplicationUserCreatedEvent>
{
    private readonly ILogger<SendStatsChangedNotificationHandler> _logger;
    private readonly INotificationSender _notifications;

    public SendStatsChangedNotificationHandler(ILogger<SendStatsChangedNotificationHandler> logger, INotificationSender notifications) =>
        (_logger, _notifications) = (logger, notifications);

    public Task Handle(EventNotification<EntityCreatedEvent<Brand>> notification, CancellationToken cancellationToken) =>
        SendStatsChangedNotificationAsync(notification.Event, cancellationToken);
    public Task Handle(EventNotification<EntityDeletedEvent<Brand>> notification, CancellationToken cancellationToken) =>
        SendStatsChangedNotificationAsync(notification.Event, cancellationToken);
    public Task Handle(EventNotification<EntityCreatedEvent<Product>> notification, CancellationToken cancellationToken) =>
        SendStatsChangedNotificationAsync(notification.Event, cancellationToken);
    public Task Handle(EventNotification<EntityDeletedEvent<Product>> notification, CancellationToken cancellationToken) =>
        SendStatsChangedNotificationAsync(notification.Event, cancellationToken);
    public Task Handle(EventNotification<ApplicationRoleCreatedEvent> notification, CancellationToken cancellationToken) =>
        SendStatsChangedNotificationAsync(notification.Event, cancellationToken);
    public Task Handle(EventNotification<ApplicationRoleDeletedEvent> notification, CancellationToken cancellationToken) =>
        SendStatsChangedNotificationAsync(notification.Event, cancellationToken);
    public Task Handle(EventNotification<ApplicationUserCreatedEvent> notification, CancellationToken cancellationToken) =>
        SendStatsChangedNotificationAsync(notification.Event, cancellationToken);

    private Task SendStatsChangedNotificationAsync(IEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{event} Triggered => Sending StatsChangedNotification", @event.GetType().Name);

        return _notifications.SendToAllAsync(new StatsChangedNotification(), cancellationToken);
    }
}
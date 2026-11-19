using Genocs.Common.CQRS.Events;
using Genocs.Common.Dependency;

namespace Genocs.Microservice.Template.Application.Common.Events;

/// <summary>
/// The event publisher interface.
/// </summary>
public interface IEventPublisher : ITransientDependency
{
    /// <summary>
    /// Publish the event.
    /// </summary>
    /// <param name="event">The event to be publish.</param>
    /// <returns>The task.</returns>
    Task PublishAsync(IEvent @event);
}
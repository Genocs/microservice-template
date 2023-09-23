using Genocs.Microservice.Shared.Events;

namespace Genocs.Microservice.Application.Common.Events;

public interface IEventPublisher : ITransientService
{
    Task PublishAsync(IEvent @event);
}
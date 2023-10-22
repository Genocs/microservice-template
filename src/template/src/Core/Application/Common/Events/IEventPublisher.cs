using Genocs.Microservice.Template.Shared.Events;

namespace Genocs.Microservice.Template.Application.Common.Events;

public interface IEventPublisher : ITransientService
{
    Task PublishAsync(IEvent @event);
}
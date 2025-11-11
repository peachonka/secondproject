namespace Shared.Models;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent;
}
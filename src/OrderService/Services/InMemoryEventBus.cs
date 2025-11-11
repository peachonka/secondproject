using Shared.Models;

namespace OrderService.Services;

public class InMemoryEventBus : IEventBus
{
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent
    {
        _logger.LogInformation("Publishing event: {EventType} {EventId}", @event.EventType, @event.EventId);
        
        // TODO: Заменить на реальную отправку в RabbitMQ/Kafka
        // Пока просто логируем событие
        switch (@event)
        {
            case OrderCreatedEvent orderCreated:
                _logger.LogInformation("Order created: {OrderId} for user {UserId}", orderCreated.OrderId, orderCreated.UserId);
                break;
            case OrderStatusUpdatedEvent statusUpdated:
                _logger.LogInformation("Order status updated: {OrderId} from {OldStatus} to {NewStatus}", 
                    statusUpdated.OrderId, statusUpdated.OldStatus, statusUpdated.NewStatus);
                break;
        }

        return Task.CompletedTask;
    }
}
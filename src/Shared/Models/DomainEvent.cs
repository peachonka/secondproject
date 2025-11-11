namespace Shared.Models;

public abstract class DomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
}

public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemEvent> Items { get; set; } = new();
}

public class OrderStatusUpdatedEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class OrderItemEvent
{
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
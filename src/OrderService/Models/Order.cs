using System.ComponentModel.DataAnnotations;

namespace OrderService.Models;

public class Order
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    public List<OrderItem> Items { get; set; } = new();
    
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum OrderStatus
{
    Created,
    InProgress, 
    Completed,
    Cancelled
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OrderService.Models;
using Shared.Models;
using System.Security.Claims;

namespace OrderService.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IEventBus _eventBus;

    public OrdersController(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Публикуем событие создания заказа
        await _eventBus.PublishAsync(new OrderCreatedEvent
        {
            OrderId = orderId,
            UserId = userId,
            TotalAmount = request.Items.Sum(i => i.Quantity * i.Price),
            Items = request.Items.Select(i => new OrderItemEvent 
            { 
                Product = i.Product, 
                Quantity = i.Quantity, 
                Price = i.Price 
            }).ToList(),
            EventType = "OrderCreated"
        });

        var response = new ApiResponse<object>
        {
            Success = true,
            Data = new { 
                Id = orderId, 
                Status = "created",
                TotalAmount = request.Items.Sum(i => i.Quantity * i.Price)
            }
        };
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var response = new ApiResponse<object>
        {
            Success = true,
            Data = new { 
                Id = id, 
                UserId = Guid.NewGuid(),
                Status = "created",
                Items = new List<object>(),
                TotalAmount = 1000m
            }
        };
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserOrders([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var response = new ApiResponse<object>
        {
            Success = true,
            Data = new { 
                Orders = new List<object>(),
                Total = 0,
                Page = page,
                Limit = limit
            }
        };
        return Ok(response);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Публикуем событие обновления статуса
        await _eventBus.PublishAsync(new OrderStatusUpdatedEvent
        {
            OrderId = id,
            UserId = userId,
            OldStatus = "created", // В реальности нужно получить текущий статус из БД
            NewStatus = request.Status.ToString(),
            EventType = "OrderStatusUpdated"
        });

        var response = new ApiResponse<object>
        {
            Success = true,
            Data = new { 
                Id = id, 
                Status = request.Status.ToString() 
            }
        };
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Публикуем событие отмены заказа
        await _eventBus.PublishAsync(new OrderStatusUpdatedEvent
        {
            OrderId = id,
            UserId = userId,
            OldStatus = "created",
            NewStatus = "cancelled",
            EventType = "OrderStatusUpdated"
        });

        var response = new ApiResponse<object>
        {
            Success = true,
            Data = new { 
                Id = id, 
                Status = "cancelled" 
            }
        };
        return Ok(response);
    }
}
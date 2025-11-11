using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderService.Controllers;
using OrderService.Models;
using OrderService.Services;
using Shared.Models;
using System.Security.Claims;
using System.Text.Json;
using Xunit;

namespace OrderService.Tests.UnitTests;

public class OrdersControllerTests
{
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly OrdersController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public OrdersControllerTests()
    {
        _eventBusMock = new Mock<IEventBus>();
        _controller = new OrdersController(_eventBusMock.Object);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Email, "test@example.com")
        }, "mock"));
        
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        _eventBusMock.Setup(x => x.PublishAsync(It.IsAny<OrderCreatedEvent>()))
                     .Returns(Task.CompletedTask);
        _eventBusMock.Setup(x => x.PublishAsync(It.IsAny<OrderStatusUpdatedEvent>()))
                     .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task CreateOrder_WithValidItems_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { Product = "Cement", Quantity = 10, Price = 500 },
                new OrderItemRequest { Product = "Sand", Quantity = 5, Price = 300 }
            }
        };

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        // Десериализуем для проверки структуры
        var json = JsonSerializer.Serialize(response.Data);
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        Assert.True(data.TryGetProperty("Id", out _));
        Assert.True(data.TryGetProperty("Status", out var status));
        Assert.True(data.TryGetProperty("TotalAmount", out var totalAmount));
        
        Assert.Equal("created", status.GetString());
        Assert.Equal(6500, totalAmount.GetDecimal());
        
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<OrderCreatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_WithEmptyItems_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>()
        };

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var json = JsonSerializer.Serialize(response.Data);
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        Assert.True(data.TryGetProperty("TotalAmount", out var totalAmount));
        Assert.Equal(0, totalAmount.GetDecimal());
    }

    [Fact]
    public async Task GetOrder_WithValidId_ReturnsOrderData()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var json = JsonSerializer.Serialize(response.Data);
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        Assert.True(data.TryGetProperty("Id", out var id));
        Assert.True(data.TryGetProperty("Status", out var status));
        Assert.True(data.TryGetProperty("UserId", out _));
        Assert.True(data.TryGetProperty("Items", out _));
        Assert.True(data.TryGetProperty("TotalAmount", out _));
        
        Assert.Equal(orderId.ToString(), id.GetString());
        Assert.Equal("created", status.GetString());
    }

    [Fact]
    public async Task GetUserOrders_WithDefaultPagination_ReturnsList()
    {
        // Act
        var result = await _controller.GetUserOrders();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var json = JsonSerializer.Serialize(response.Data);
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        Assert.True(data.TryGetProperty("Orders", out _));
        Assert.True(data.TryGetProperty("Total", out var total));
        Assert.True(data.TryGetProperty("Page", out var page));
        Assert.True(data.TryGetProperty("Limit", out var limit));
        
        Assert.Equal(0, total.GetInt32());
        Assert.Equal(1, page.GetInt32());
        Assert.Equal(10, limit.GetInt32());
    }

    [Fact]
    public async Task GetUserOrders_WithCustomPagination_ReturnsCorrectPagination()
    {
        // Arrange
        var page = 2;
        var limit = 5;

        // Act
        var result = await _controller.GetUserOrders(page, limit);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var json = JsonSerializer.Serialize(response.Data);
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        Assert.True(data.TryGetProperty("Page", out var actualPage));
        Assert.True(data.TryGetProperty("Limit", out var actualLimit));
        
        Assert.Equal(page, actualPage.GetInt32());
        Assert.Equal(limit, actualLimit.GetInt32());
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidStatus_ReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest { Status = OrderStatus.InProgress };

        // Act
        var result = await _controller.UpdateOrderStatus(orderId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var json = JsonSerializer.Serialize(response.Data);
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        Assert.True(data.TryGetProperty("Id", out var id));
        Assert.True(data.TryGetProperty("Status", out var status));
        
        Assert.Equal(orderId.ToString(), id.GetString());
        Assert.Equal("InProgress", status.GetString());
        
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<OrderStatusUpdatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrder_WithValidId_ReturnsCancelledStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var result = await _controller.CancelOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var json = JsonSerializer.Serialize(response.Data);
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        Assert.True(data.TryGetProperty("Id", out var id));
        Assert.True(data.TryGetProperty("Status", out var status));
        
        Assert.Equal(orderId.ToString(), id.GetString());
        Assert.Equal("cancelled", status.GetString());
        
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<OrderStatusUpdatedEvent>()), Times.Once);
    }

    [Theory]
    [InlineData(OrderStatus.Created)]
    [InlineData(OrderStatus.InProgress)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task UpdateOrderStatus_WithAllStatuses_ReturnsSuccess(OrderStatus status)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest { Status = status };

        // Act
        var result = await _controller.UpdateOrderStatus(orderId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        var json = JsonSerializer.Serialize(response.Data);
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        Assert.True(data.TryGetProperty("Status", out var actualStatus));
        Assert.Equal(status.ToString(), actualStatus.GetString());
    }
}
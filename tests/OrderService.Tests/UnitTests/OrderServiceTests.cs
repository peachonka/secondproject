using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderService.Controllers;
using OrderService.Models;
using OrderService.Services;
using Shared.Models;
using System.Security.Claims;
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
        
        // Setup user context
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Email, "test@example.com")
        }, "mock"));
        
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        // Настраиваем мок для конкретных типов событий
        _eventBusMock.Setup(x => x.PublishAsync(It.IsAny<OrderCreatedEvent>()))
                     .Returns(Task.CompletedTask);
        _eventBusMock.Setup(x => x.PublishAsync(It.IsAny<OrderStatusUpdatedEvent>()))
                     .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task CreateOrder_WithValidItems_ReturnsSuccessWithOrderData()
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
        
        // Используем dynamic для проверки структуры ответа
        dynamic data = response.Data!;
        Guid id = data.Id;
        string status = data.Status;
        decimal totalAmount = data.TotalAmount;
        
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("created", status);
        Assert.Equal(6500, totalAmount); // 10*500 + 5*300
        
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<OrderCreatedEvent>(e => 
            e.UserId == _testUserId &&
            e.TotalAmount == 6500 &&
            e.Items.Count == 2
        )), Times.Once);
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
        
        dynamic data = response.Data!;
        decimal totalAmount = data.TotalAmount;
        Assert.Equal(0, totalAmount);
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
        
        dynamic data = response.Data!;
        Guid id = data.Id;
        string status = data.Status;
        Guid userId = data.UserId;
        object items = data.Items;
        decimal totalAmount = data.TotalAmount;
        
        Assert.Equal(orderId, id);
        Assert.Equal("created", status);
        Assert.NotEqual(Guid.Empty, userId);
        Assert.NotNull(items);
        Assert.True(totalAmount >= 0);
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
        
        dynamic data = response.Data!;
        object orders = data.Orders;
        int total = data.Total;
        int page = data.Page;
        int limit = data.Limit;
        
        Assert.NotNull(orders);
        Assert.Equal(0, total);
        Assert.Equal(1, page);
        Assert.Equal(10, limit);
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
        
        dynamic data = response.Data!;
        int actualPage = data.Page;
        int actualLimit = data.Limit;
        
        Assert.Equal(page, actualPage);
        Assert.Equal(limit, actualLimit);
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
        
        dynamic data = response.Data!;
        Guid id = data.Id;
        string status = data.Status;
        
        Assert.Equal(orderId, id);
        Assert.Equal("InProgress", status);
        
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<OrderStatusUpdatedEvent>(e => 
            e.OrderId == orderId &&
            e.UserId == _testUserId &&
            e.NewStatus == "InProgress"
        )), Times.Once);
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
        
        dynamic data = response.Data!;
        Guid id = data.Id;
        string status = data.Status;
        
        Assert.Equal(orderId, id);
        Assert.Equal("cancelled", status);
        
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<OrderStatusUpdatedEvent>(e => 
            e.OrderId == orderId &&
            e.NewStatus == "cancelled"
        )), Times.Once);
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
        
        dynamic data = response.Data!;
        string actualStatus = data.Status;
        Assert.Equal(status.ToString(), actualStatus);
    }
}
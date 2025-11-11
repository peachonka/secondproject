using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Models;

namespace OrderService.Tests;

public class OrderServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ForAuthenticatedUser_ReturnsSuccess()
    {
        // TODO: Добавить аутентификацию
        var request = new
        {
            Items = new[] {
                new { Product = "Cement", Quantity = 10, Price = 500 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result?.Success);
    }

    [Fact]
    public async Task GetOrder_ReturnsOrderDetails()
    {
        var orderId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/v1/orders/{orderId}");
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result?.Success);
    }
}
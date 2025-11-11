using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiGateway.Tests;

public class ApiGatewayIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiGatewayIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Proxy_UsersRoute_ForwardsToUserService()
    {
        var response = await _client.GetAsync("/users/profile");
        // Проверяем что запрос проксируется (может возвращать 401 из-за отсутствия токена)
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task Proxy_OrdersRoute_ForwardsToOrderService()
    {
        var response = await _client.GetAsync("/orders");
        // Проверяем что запрос проксируется
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.OK);
    }
}
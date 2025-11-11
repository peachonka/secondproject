using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Models;

namespace UserService.Tests;

public class UserServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        var request = new
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "Password123!",
            Name = "Test User"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result?.Success);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsError()
    {
        var request = new
        {
            Email = "duplicate@example.com",
            Password = "Password123!",
            Name = "Test User"
        };

        await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.False(result?.Success);
        Assert.Equal("USER_EXISTS", result?.Error?.Code);
    }
}
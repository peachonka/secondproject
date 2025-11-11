using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserService.Data;
using UserService.Models;
using UserService.Services.Implementations;
using Xunit;

namespace UserService.Tests.UnitTests;

public class AuthServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        
        _context = new ApplicationDbContext(options);
        
        // Создаем конфигурацию для тестов
        var inMemorySettings = new Dictionary<string, string?> {
            {"Jwt:Secret", "test-secret-key-that-is-long-enough-for-testing"}
        };
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
            
        _authService = new AuthService(_context, _configuration);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            Name = "Test User"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.UserId);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.Name);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "Password123!",
            Name = "Test User"
        };

        // First registration
        await _authService.RegisterAsync(request);

        // Second registration with same email
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User already exists", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "login@example.com",
            Password = "Password123!",
            Name = "Test User"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "login@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.UserId);
        Assert.Equal("login@example.com", result.Email);
        Assert.Equal("Test User", result.Name);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsError()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password123!",
            Name = "Test User"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPassword"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid credentials", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsError()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid credentials", result.Error);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Models;
using Shared.Models; 
using UserService.Services.Interfaces;


namespace UserService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
/// <summary>
/// Регистрация нового пользователя
/// </summary>
/// <param name="request">Данные для регистрации</param>
/// <returns>Результат регистрации с JWT токеном</returns>
   [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    var result = await _authService.RegisterAsync(request);
    
    if (!result.Success)
        return Conflict(ApiResponse<object>.ErrorResult("USER_EXISTS", result.Error!));

    var response = new ApiResponse<AuthResponse>
    {
        Success = true,
        Data = new AuthResponse { 
            UserId = result.UserId!.Value, 
            Token = result.Token!,
            Email = request.Email,
            Name = request.Name
        }
    };
    return Ok(response);
}

[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var result = await _authService.LoginAsync(request);
    
    if (!result.Success)
        return Unauthorized(ApiResponse<object>.ErrorResult("INVALID_CREDENTIALS", result.Error!));

    var response = new ApiResponse<AuthResponse>
    {
        Success = true,
        Data = new AuthResponse { 
            UserId = result.UserId!.Value, 
            Token = result.Token!,
            Email = request.Email,
        }
    };
    return Ok(response);
}
}
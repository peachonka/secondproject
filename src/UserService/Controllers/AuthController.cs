using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Models;
using Shared.Models; 

namespace UserService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // TODO: Реализация регистрации
        var response = new ApiResponse<object>
        {
            Success = true,
            Data = new { UserId = Guid.NewGuid(), Email = request.Email }
        };
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // TODO: Реализация входа
        var response = new ApiResponse<object>
        {
            Success = true,
            Data = new { Token = "jwt-token-here" }
        };
        return Ok(response);
    }
}
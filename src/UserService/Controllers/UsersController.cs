using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Models;
using Shared.Models; 
namespace UserService.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // TODO: Получение профиля
        var response = new ApiResponse<object>
        {
            Success = true,
            Data = new { Name = "Test User", Email = "test@example.com" }
        };
        return Ok(response);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        // TODO: Обновление профиля
        var response = new ApiResponse<object>
        {
            Success = true,
            Data = new { Message = "Profile updated" }
        };
        return Ok(response);
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        // TODO: Список пользователей для админа
        var response = new ApiResponse<object>
        {
            Success = true,
            Data = new { Users = new List<object>(), Total = 0, Page = page }
        };
        return Ok(response);
    }
}
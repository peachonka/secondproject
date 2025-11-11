using UserService.Models;

namespace UserService.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(RegisterRequest request);
    Task UpdateUserAsync(User user);
}
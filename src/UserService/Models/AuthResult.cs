namespace UserService.Models;
public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Error { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Guid? UserId { get; set; }
}
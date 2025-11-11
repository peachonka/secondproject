namespace Shared.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ApiError? Error { get; set; }
    
    public static ApiResponse<T> SuccessResult(T data) => new() { Success = true, Data = data };
    public static ApiResponse<object> ErrorResult(string code, string message) => new() 
    { 
        Success = false, 
        Error = new ApiError { Code = code, Message = message } 
    };
}
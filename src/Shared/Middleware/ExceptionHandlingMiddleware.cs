using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Shared.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    var response = new ApiResponse<object>
    {
        Success = false,
        Error = new ApiError
        {
            Code = "INTERNAL_ERROR", 
            Message = "An error occurred while processing your request"
        }
    };

    context.Response.StatusCode = 500;
    context.Response.ContentType = "application/json";
    return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
}
}
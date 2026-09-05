using System.Net;
using System.Text.Json;
using TaskManagement.Server.Common;

namespace TaskManagement.Server.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (BadHttpRequestException ex)
        {
            _logger.LogWarning(ex, "Bad HTTP Request: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized Access Attempt: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, AppMessages.Auth.Unauthorized);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource Not Found: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid Business Operation: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception: {Message} | Path: {Path}", ex.Message, context.Request.Path);
            await HandleExceptionAsync(
                context, 
                HttpStatusCode.InternalServerError, 
                AppMessages.Errors.InternalServerError, 
                new List<string> { ex.Message }
            );
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context, 
        HttpStatusCode statusCode, 
        string message, 
        List<string>? errors = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.Fail(message, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}

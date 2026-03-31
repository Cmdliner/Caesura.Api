using System.Net;
using System.Text.Json;

namespace Caesura.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled Exception");
            await HandleExceptionAsync(context, e);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occured")
        };
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = message,
            status = (int) status
        }));
    }
}
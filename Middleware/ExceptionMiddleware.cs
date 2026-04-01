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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, logger);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context, Exception ex, ILogger logger)
    {
        // Map exception types to HTTP status codes and messages
        var (status, message, shouldLog) = ex switch
        {
            // 401 — authentication failures
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message, false),

            // 400 — caller errors: missing body, null model, bad arguments
            // ArgumentNullException covers FluentValidation's null model guard
            // and also bad Guid parsing, etc.
            ArgumentNullException e =>
                (HttpStatusCode.BadRequest,
                 e.ParamName == "instanceToValidate"
                    ? "Request body is required."
                    : $"Invalid input: {e.ParamName} is required.",
                 false),

            ArgumentException =>
                (HttpStatusCode.BadRequest, ex.Message, false),

            // 409 — business rule conflicts (duplicate email, username, etc.)
            InvalidOperationException => (HttpStatusCode.Conflict, ex.Message, false),

            // 404 — explicit not found
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message, false),

            // 500 — everything else is unexpected; log it
            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred.",
                  true)
        };

        if (shouldLog)
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        else
            logger.LogWarning("Handled exception [{Type}]: {Message}",
                ex.GetType().Name, ex.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var body = JsonSerializer.Serialize(
            new { error = message, status = (int)status },
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

        return context.Response.WriteAsync(body);
    }
}